using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Factories;
using VVVV.Hosting.IO;

namespace VVVV.DX11.RenderGraph.Model
{
    /// <summary>
    /// Wraps vvvv node info and instance
    /// </summary>
    public class DX11Node
    {
        public DX11Node(INode2 hdeNode) : this(hdeNode, (IPluginHost)hdeNode.InternalCOMInterf)
        {
        }

        public DX11Node(INode2 hdeNode, IPluginHost hoster)
        {
            this.InputPins = new List<DX11InputPin>();
            this.OutputPins = new List<DX11OutputPin>();

            this.HdeNode2 = hdeNode;
            this.HdeNode = hdeNode.InternalCOMInterf;
            this.Name = hdeNode.NodeInfo.Systemname;
            this.Hoster = hoster;

            IInternalPluginHost iip = (IInternalPluginHost)this.Hoster;
            this.Interfaces = new DX11NodeInterfaces(iip);
            this.VirtualConnections = new List<DX11VirtualConnection>();
        }

        public IPluginHost Hoster { get; protected set; }
        public INode HdeNode { get; protected set; }
        public INode2 HdeNode2 { get; private set; }
        public DX11NodeInterfaces Interfaces { get; private set; }

        public string Name { get; set; }
        public string DescriptiveName
        {
            get
            {
                return this.HdeNode.GetPin("Descriptive Name").GetSlice(0);
            }
        }

        public List<DX11VirtualConnection> VirtualConnections;

        public DX11VirtualConnection GetVirtualConnection(IPin pin)
        {
            foreach (DX11VirtualConnection connection in this.VirtualConnections)
            {
                if (connection.sinkPin == pin)
                {
                    return connection;
                }
            }
            return null;
        }

        public List<DX11InputPin> InputPins { get; set; }
        public List<DX11OutputPin> OutputPins { get; set; }

        public DX11InputPin GetInput(string name)
        {
            foreach (DX11InputPin ip in this.InputPins)
            {
                if (ip.Name == name) { return ip; }
            }
            return null;
        }

        public DX11OutputPin GetOutput(string name)
        {
            foreach (DX11OutputPin ip in this.OutputPins)
            {
                if (ip.Name == name) { return ip; }
            }
            return null;
        }


        public DX11InputPin GetInput(IPin pin)
        {
            foreach (DX11InputPin ip in this.InputPins)
            {
                if (ip.HdePin == pin) { return ip; }
            }
            return null;
        }

        public DX11OutputPin GetOutput(IPin pin)
        {
            foreach (DX11OutputPin ip in this.OutputPins)
            {
                if (ip.HdePin == pin) { return ip; }
            }
            return null;
        }

        public bool RemovePin(string name, PinDirection direction)
        {
            if (direction == PinDirection.Input)
            {
                DX11InputPin ip = null;
                foreach (DX11InputPin vi in this.InputPins)
                {
                    if (vi.Name == name)
                    {
                        ip = vi;
                    }
                }

                if (ip != null)
                {
                    //Diconnect parent if applicable
                    if (ip.ParentPin != null)
                    {
                        if (ip.ParentPin.ChildrenPins.Contains(ip))
                        {
                            ip.ParentPin.ChildrenPins.Remove(ip);
                        }
                    }

                    this.InputPins.Remove(ip);
                }

                return ip != null;
            }

            if (direction == PinDirection.Output)
            {
                DX11OutputPin op = null;
                foreach (DX11OutputPin vo in this.OutputPins)
                {
                    if (vo.Name == name)
                    {
                        op = vo;
                    }
                }
                if (op != null)
                {
                    foreach (DX11InputPin vip in op.ChildrenPins)
                    {
                        vip.ParentPin = null;
                    }
                    op.ChildrenPins.Clear();

                    this.OutputPins.Remove(op);
                }

                return op != null;
            }

            return false;
        }

    }
}
