﻿using System;
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
                        IDX11ResourceProvider provider = unused.ParentNode.Instance<IDX11ResourceProvider>();
                        provider.Destroy(unused.PluginIO, this.context, false);
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
            if (node.IsAssignable<IDX11UpdateBlocker>())
            {
                if (!node.Instance<IDX11UpdateBlocker>().Enabled) 
                {
                    //Add to processed list and early exit on branch.
                    this.processed.Add(node);
                    return; 
                }
            }

            //Got to all parents recursively (eg: make sure all is updated)
            foreach (DX11InputPin ip in node.InputPins)
            {
                if (ip.IsConnected && (ip.ParentPin.IsFeedBackPin == false))
                {
                    this.ProcessNode(ip.ParentPin.ParentNode);
                }
            }

            //Call Update
            foreach (DX11InputPin ip in node.InputPins)
            {
                if (ip.IsConnected)
                {
                    DX11OutputPin parent = ip.ParentPin;

                    if (!this.thisframepins.Contains(parent))
                    {
                        DX11Node source = parent.ParentNode;

                        IDX11ResourceProvider provider = source.Instance<IDX11ResourceProvider>();

                        try
                        {
                            provider.Update(parent.PluginIO, this.context);

                            if (source.IsAssignable<IDX11MultiResourceProvider>())
                            {
                                if (this.DoNotDestroy == false)
                                {
                                    //Mark all output pins as processed
                                    foreach (DX11OutputPin outpin in source.OutputPins)
                                    {
                                        this.thisframepins.Add(outpin);
                                    }
                                }
                            }
                            else
                            {
                                if (this.DoNotDestroy == false)
                                {
                                    //Mark output pin as used this frame
                                    this.thisframepins.Add(parent);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            this.logger.Log(ex);
                            //Log 
                        }

                        if (this.DoNotDestroy == false)
                        {
                            //Remove from old cache if applicable
                            if (this.lastframepins.Contains(parent))
                            {
                                this.lastframepins.Remove(parent);
                            }
                        }
                    }
                }

            }

            //Render if renderer
            if (node.IsAssignable<IDX11RendererProvider>())
            {
                try
                {
                    IDX11RendererProvider provider = node.Instance<IDX11RendererProvider>();
                    provider.Render(this.context);
                }
                catch (Exception ex)
                {
                    this.logger.Log(ex);
                }
            }

            //Node fully processed
            this.processed.Add(node);
        }
        #endregion

        #region Find Renderers
        private List<DX11Node> FindRenderers()
        {
            List<DX11Node> renderers = new List<DX11Node>();

            foreach (DX11Node n in this.graph.Nodes)
            {
                if (n.IsAssignable<IDX11RendererProvider>())
                {
                    renderers.Add(n);
                }
            }
            return renderers;
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            foreach (DX11Node node in this.graph.Nodes)
            {
                foreach (DX11OutputPin outpin in node.OutputPins)
                {
                    //Call destroy
                    IDX11ResourceProvider provider = outpin.ParentNode.Instance<IDX11ResourceProvider>();

                    try
                    {
                        provider.Destroy(outpin.PluginIO, this.context, true);
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
