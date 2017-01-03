using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using VVVV.PluginInterfaces.V2.Graph;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting;

using VVVV.DX11.Lib;

using VVVV.DX11.RenderGraph.Model;
using VVVV.DX11.Lib.Devices;
using VVVV.Nodes;


namespace VVVV.DX11.Lib.RenderGraph.Listeners
{
    class HdeLink
    {
        public HdeLink(IPin sink, IPin src) { this.sink = sink; this.src = src; }

        public IPin sink;
        public IPin src;
    }

    public interface IDX11GraphBuilder
    {
        void Flush();
        DX11Graph Graph { get; }
    }

    public class DX11GraphBuilder : AbstractHdeNodeListener, IDX11GraphBuilder, IDX11RenderDependencyFactory
    {
        private DX11Graph graph;

        public DX11Graph Graph { get { return this.graph; } }

        private List<IPin2> pendingpins = new List<IPin2>();
        private Dictionary<IPin, HdeLink> pendinglinks = new Dictionary<IPin, HdeLink>();
        private List<Tuple<IPin, IPin>> pendingDependencies = new List<Tuple<IPin, IPin>>(); 

        public event DX11RenderRequestDelegate RenderRequest;

        private DX11ResourceRegistry reg;

        public int PendingPinCount { get { return this.pendingpins.Count; } }
        public int PendingLinkCount { get { return this.pendinglinks.Count; } }

        public DX11GraphBuilder(IHDEHost hde, DX11ResourceRegistry reg)
            : base(hde)
        {
            this.graph = new DX11Graph();
            this.reg = reg;

        }


        /// <summary>
        /// Flush pending pins (to avoid listener bug)
        /// </summary>
        public void Flush()
        {
            foreach (IPin2 pin in this.pendingpins)
            {
                this.AddPin(pin);
            }
            this.pendingpins.Clear();

            foreach (Tuple<IPin, IPin> dep in this.pendingDependencies)
            {
                this.CreateAdditionalConnection(dep.Item1, dep.Item2);
            }
            this.pendingpins.Clear();
            this.pendingDependencies.Clear();

            this.ProcessPendingLinks();
        }

        public void CreateDependency(IPin inputPin, IPin outputPin)
        {
            this.pendingDependencies.Add(new Tuple<IPin, IPin>(inputPin, outputPin));
        }

        public void DeleteDependency(IPin inputPin)
        {
            this.RemoveAdditionalConnection(inputPin);
        }

        protected override bool ProcessAddedNode(INode2 node)
        {

            if (node.IsNodeAssignableFrom<IDX11RenderGraphPart>() || node.IsNodeAssignableFrom<IDX11ResourceDataRetriever>())
            {
                DX11Node vn = new DX11Node(node);

                this.graph.AddNode(vn);

                //If force updater, register event
                if (node.IsNodeAssignableFrom<IDX11ResourceDataRetriever>())
                {
                    IDX11ResourceDataRetriever updater = vn.Interfaces.DataRetriever;
                    updater.RenderRequest += OnRenderRequest;
                }

                foreach (IPin2 p in node.Pins)
                {
                    this.ProcessAddedPin(p,true);
                }
                return true;
            }

            return false;
        }

        private void OnRenderRequest(IDX11ResourceDataRetriever sender, IPluginHost host)
        {
            if (this.RenderRequest != null) { this.RenderRequest(sender, host); }
        }


        protected override bool ProcessRemovedNode(INode2 node)
        {
            DX11Node vn = this.graph.FindNode(node);

            if (vn != null)
            {
                foreach (IPin2 pin in node.Pins)
                {
                    this.ProcessRemovedPin(pin);
                }
                this.graph.RemoveNode(vn);
                return true;
            }
            return false;
        }

        private void AddPin(IPin2 pin)
        {
            DX11Node vn = this.graph.FindNode(pin.ParentNode);

            IPluginIO io = pin.InternalCOMInterf as IPluginIO;

            if (pin.Direction == PinDirection.Input && io != null && pin.Type.StartsWith("DX11Resource"))
            {
                DX11InputPin vop = new DX11InputPin(vn, pin.InternalCOMInterf, io);

                //We only register event on input pin (more than enough to build graph)
                pin.Connected += pin_Connected;
                pin.Disconnected += pin_Disconnected;

                IPin src = this.GetSource(pin.InternalCOMInterf);

                if (src != null)
                {
                    if (this.CanProcessPin(pin.InternalCOMInterf, src))
                    {
                        if (!this.SetLink(pin.InternalCOMInterf, src,false))
                        {
                            //Add projected pending link
                            HdeLink link = new HdeLink(pin.InternalCOMInterf, src);
                            this.pendinglinks.Add(pin.InternalCOMInterf,link);
                        }
                    }
                }
            }

            if (pin.Direction == PinDirection.Output && io != null && pin.Type.StartsWith("DX11Resource"))
            {
                DX11OutputPin vop = new DX11OutputPin(vn, pin.InternalCOMInterf, io);
            }
        }

        private IPin GetSource(IPin sink)
        {
            //Normally we should have data from cache (since it's created before events are fired)
            if (reg.pinconnections.ContainsKey(sink))
            {
                return reg.pinconnections[sink].ParentPin;
            }
            return null;
        }

        private bool CanProcessPin(IPin source, IPin sink)
        {
            //TODO: Proper type filter here, need to avoid set link on getslice/switch
            return source.CLRType != null && sink.CLRType != null; 
        }

        private void pin_Disconnected(object sender, PinConnectionEventArgs args)
        {
            IPin2 sink = sender as IPin2;
            this.UnSetLink(sink.InternalCOMInterf, args.OtherPin);          
        }

        private void pin_Connected(object sender, PinConnectionEventArgs args)
        {
            IPin2 sink = sender as IPin2;
            this.SetLink(sink.InternalCOMInterf, args.OtherPin,false);
        }

        private void RemoveAdditionalConnection(IPin sink)
        {
            if (sink as IPluginIO == null)
                return;

            DX11Node sinknode = this.graph.FindNode(sink.ParentNode);

            if (sinknode != null)
            {
                DX11VirtualConnection connection = sinknode.GetVirtualConnection(sink);
                if (connection != null)
                {
                    sinknode.VirtualConnections.Remove(connection);
                }
            }
        }

        private void CreateAdditionalConnection(IPin sink, IPin source)
        {
            if (sink as IPluginIO == null || source as IPluginIO == null)
                return;

            DX11Node sinknode = this.graph.FindNode(sink.ParentNode);
            DX11Node sourcenode = this.graph.FindNode(source.ParentNode);

            if (sinknode != null && sourcenode != null)
            {
                DX11VirtualConnection existing = sinknode.GetVirtualConnection(sink);
                if (existing != null)
                {
                    sinknode.VirtualConnections.Remove(existing);
                }

                DX11VirtualConnection connection = new DX11VirtualConnection(sink, source, sourcenode);
                sinknode.VirtualConnections.Add(connection);
            }
        }

        private bool SetLink(IPin sink,IPin source, bool frompending)
        {
            DX11Node sinknode = this.graph.FindNode(sink.ParentNode);
            DX11Node sourcenode = this.graph.FindNode(source.ParentNode);

            if (sinknode != null && sourcenode != null)
            {
                DX11InputPin sinkpin = sinknode.GetInput(sink.Name);
                DX11OutputPin sourcepin = sourcenode.GetOutput(source.Name);
                sinkpin.Connect(sourcepin);

                //Since we managed to connect, check if it exists in pending list and remove

                if (!frompending)
                {
                    if (this.pendinglinks.ContainsKey(sink))
                    {
                        this.pendinglinks.Remove(sink);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void ProcessPendingLinks()
        {
            //Try to see if we can process lost links now
            Dictionary<IPin, HdeLink> np = new Dictionary<IPin, HdeLink>();

            foreach (IPin p in this.pendinglinks.Keys)
            {
                HdeLink l = this.pendinglinks[p];
                if (!this.SetLink(l.sink,l.src,true))
                {
                    np[p] = l;
                }
            }
            this.pendinglinks = np;
        }

        private void UnSetLink(IPin sink, IPin source)
        {
            DX11Node sinknode = this.graph.FindNode(sink.ParentNode);
            DX11Node sourcenode = this.graph.FindNode(source.ParentNode);

            if (sinknode != null && sourcenode != null)
            {
                DX11InputPin sinkpin = sinknode.GetInput(sink.Name);
                DX11OutputPin sourcepin = sourcenode.GetOutput(source.Name);

                sinkpin.Disconnect(sourcepin);
            }
        }

        #region Process Added Pin
        protected override bool ProcessAddedPin(IPin2 pin, bool immediate)
        {
            DX11Node vn = this.graph.FindNode(pin.ParentNode);

            if (vn != null)
            {
                if (immediate)
                {
                    this.AddPin(pin);
                }
                else
                {
                    this.pendingpins.Add(pin);
                }
            }

            return false;
        }
        #endregion

        #region Process Removed Pin
        protected override bool ProcessRemovedPin(IPin2 pin)
        {
            DX11Node vn = this.graph.FindNode(pin.ParentNode);

            if (vn != null)
            {
                vn.RemovePin(pin.InternalCOMInterf.Name, pin.InternalCOMInterf.Direction);

                if (reg.pinconnections.ContainsKey(pin.InternalCOMInterf))
                {
                    reg.pinconnections.Remove(pin.InternalCOMInterf);
                }
            }
            return false;
        }
        #endregion
    }
}
