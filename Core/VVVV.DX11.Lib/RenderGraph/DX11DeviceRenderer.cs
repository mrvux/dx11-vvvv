using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;

using VVVV.DX11.Lib.Devices;
using VVVV.DX11.RenderGraph.Model;

using SlimDX.Direct3D11;

using FeralTic.DX11;

namespace VVVV.DX11.Lib.RenderGraph
{
    /// <summary>
    /// Per device renderer
    /// </summary>
    public class DX11DeviceRenderer : IDisposable
    {
        private DX11Graph graph;
        private DX11RenderContext context;

        //List of nodes for early exit
        private List<DX11Node> processed = new List<DX11Node>();

        //History of resouce usage per frame
        private List<DX11OutputPin> lastframepins = new List<DX11OutputPin>();
        private List<DX11OutputPin> thisframepins = new List<DX11OutputPin>();

        public int LastPinsCount
        {
            get { return this.lastframepins.Count; }
        }

        public int ThisFramePins
        {
            get { return this.thisframepins.Count; }
        }

        public bool DoNotDestroy
        {
            get;
            set;
        }

        public int ProcessedNodes
        {
            get;
            protected set;
        }

        public DX11Graph Graph
        {
            get { return this.graph; }
        }

        private ILogger logger;


        public DX11DeviceRenderer(DX11RenderContext context, DX11Graph graph, ILogger logger)
        {
            this.logger = logger;
            this.context = context;
            this.graph = graph;
            this.DoNotDestroy = false;

        }

        /// <summary>
        /// Called at beginning of frame
        /// </summary>
        public void Reset()
        {
            this.context.BeginFrame();

            //Reset list of processed nodes
            this.processed.Clear();

            //Clear frame pins
            this.thisframepins.Clear();

            if (this.DoNotDestroy)
            {
                this.thisframepins.Clear();
            }
        }

        public void EndFrame()
        {
            this.ProcessedNodes = this.processed.Count;
            this.context.EndFrame();

            if (this.context.RenderStateStack.Count > 0)
            {
                logger.Log(LogType.Warning, "Render State Stack should now have a size of 0!");
                logger.Log(LogType.Message, "Clearing");
                context.RenderStateStack.Reset();
            }

            if (this.context.RenderTargetStack.StackCount > 0)
            {
                logger.Log(LogType.Error, "Render Target Stack should now have a size of 0!");
            }
        }

        public void Render(IDX11ResourceDataRetriever sender, IPluginHost host)
        {
            //Assign device to sender
            sender.AssignedContext = this.context;

            //Called by stuff like info
            DX11Node node = this.graph.FindNode(host);

            this.ProcessNode(node);
        }


        #region Render
        public void Render(List<DX11Node> windownodes)
        {
            foreach (DX11Node windownode in windownodes)
            {
                this.ProcessNode(windownode);
            }

            if (this.DoNotDestroy == false)
            {
                //Call destroy on any pin which has not being used
                foreach (DX11OutputPin unused in this.lastframepins)
                {
                    //Check here, at end of render the com objects are already dead
                    try
                    {
                        //In case node has been deleted, we already called dispose
                        if (this.graph.Nodes.Contains(unused.ParentNode))
                        {
                            if (unused.ParentNode.Interfaces.IsResourceHost)
                            {
                                unused.ParentNode.Interfaces.ResourceHost.Destroy(this.context, false);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        this.logger.Log(ex);
                    }
                }

                //Swap pin buffers for next frame
                List<DX11OutputPin> temp = this.thisframepins;
                this.thisframepins = this.lastframepins;
                this.lastframepins = temp;
            }
        }
        #endregion

        #region Process Node
        private void ProcessNode(DX11Node node)
        {
            //Node already processed
            if (this.processed.Contains(node)) { return; }

            //Node can block processing and do early graph cut
            if (node.Interfaces.IsUpdateBlocker)
            {
                if (!node.Interfaces.UpdateBlocker.Enabled) 
                {
                    //Add to processed list and early exit on branch.
                    this.processed.Add(node);
                    return; 
                }
            }

            //Recurse upper nodes and virtual connctions
            for (int i = 0; i < node.InputPins.Count; i++)
            {
                DX11InputPin ip = node.InputPins[i];
                if (ip.IsConnected && (ip.ParentPin.IsFeedBackPin == false))
                {
                    this.ProcessNode(ip.ParentPin.ParentNode);
                }
            }
            for (int i = 0; i < node.VirtualConnections.Count; i++)
            {
                this.ProcessNode(node.VirtualConnections[i].sourceNode);
            }

            //Node should be ready, now update
            this.UpdateNode(node);

            this.RenderNode(node);

            //Node fully processed
            this.processed.Add(node);
        }
        #endregion

        private void UpdateNode(DX11Node node)
        {
            try
            {
                if (node.Interfaces.IsResourceHost)
                {
                    node.Interfaces.ResourceHost.Update(this.context);
                    if (this.DoNotDestroy == false)
                    {
                        //Mark all output pins as processed
                        foreach (DX11OutputPin outpin in node.OutputPins)
                        {
                            this.thisframepins.Add(outpin);

                            //Remove from old cache if applicable
                            if (this.lastframepins.Contains(outpin))
                            {
                                this.lastframepins.Remove(outpin);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(LogType.Error, "Exception caused by node during update :" + node.HdeNode.GetNodePath(false));
                this.logger.Log(LogType.Error, "Exception node name :" + node.HdeNode2.Name);
                this.logger.Log(ex);
                this.logger.Log(LogType.Message, "Stack Trace");
                this.logger.Log(LogType.Message, ex.StackTrace);
            }
        }

        private void RenderNode(DX11Node node)
        {
            //Render if renderer
            if (node.Interfaces.IsRendererHost)
            {
                try
                {
                    if (node.Interfaces.IsRendererHost)
                    {
                        node.Interfaces.RendererHost.Render(this.context);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Log(LogType.Error, "Exception caused by node during render :" + node.HdeNode.GetNodePath(false));
                    this.logger.Log(LogType.Error, "Node name :" + node.HdeNode2.Name);
                    this.logger.Log(ex);
                    this.logger.Log(LogType.Message, "Stack Trace");
                    this.logger.Log(LogType.Message, ex.StackTrace);
                }
            }
        }

        #region Dispose
        public void Dispose()
        {
            foreach (DX11Node node in this.graph.Nodes)
            {
                if (node.Interfaces.IsResourceHost)
                {
                    try
                    {
                        node.Interfaces.ResourceHost.Destroy(this.context, true);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(ex);
                    }
                }
            }

        }
        #endregion
    }
}
