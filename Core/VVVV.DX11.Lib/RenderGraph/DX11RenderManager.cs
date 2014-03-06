using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.DX11.Lib.Devices;
using VVVV.DX11.RenderGraph.Model;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using VVVV.Core.Logging;
using VVVV.DX11.Lib.RenderGraph.Listeners;

namespace VVVV.DX11.Lib.RenderGraph
{
    public class DX11RenderManager
    {
        public Dictionary<DX11RenderContext, DX11DeviceRenderer> RenderGraphs { get; protected set; }

        private DX11DeviceAllocator allocator;

        private IDX11RenderContextManager devmanager;

        private DX11Graph graph;
        private ILogger logger;
        private IDX11GraphBuilder gb;

        private List<IDX11RenderWindow> oldwindows = new List<IDX11RenderWindow>();

        public bool Enabled { get; set; }

        public DX11RenderManager(IDX11RenderContextManager devmanager, IDX11GraphBuilder builder, ILogger logger)
        {
            this.Enabled = true;
            this.gb = builder;
            this.RenderGraphs = new Dictionary<DX11RenderContext, DX11DeviceRenderer>();

            this.devmanager = devmanager;
            this.devmanager.RenderContextCreated += this.RenderContextCreated;

            this.allocator = new DX11DeviceAllocator(devmanager);
            this.allocator.RenderContextDisposing += this.RenderContextDisposing;

            this.graph = builder.Graph;
            this.logger = logger;

            foreach (DX11RenderContext context in this.devmanager.RenderContexts)
            {
                this.RenderGraphs.Add(context, new DX11DeviceRenderer(context, this.graph,this.logger));
            }
        }

        private void RenderContextCreated(DX11RenderContext context)
        {
            if (!this.RenderGraphs.ContainsKey(context))
            {
                this.RenderGraphs.Add(context, new DX11DeviceRenderer(context, this.graph,this.logger));
            }
        }

        private void RenderContextDisposing(DX11RenderContext context)
        {
            if (this.RenderGraphs.ContainsKey(context))
            {
                this.RenderGraphs[context].Dispose();
            }
            this.RenderGraphs.Remove(context);
        }

        public bool DoNotDestroy
        {
            set
            {
                foreach (DX11RenderContext dev in this.RenderGraphs.Keys)
                {
                    this.RenderGraphs[dev].DoNotDestroy = value;
                }
            }
        }

        public void Reset()
        {
            
            List<IDX11RenderWindow> windows = this.FindRenderWindows();
            foreach (IDX11RenderWindow win in windows)
            {
                this.allocator.AddRenderWindow(win);
            }

            foreach (IDX11RenderWindow old in this.oldwindows)
            {
                if (!windows.Contains(old)) { this.allocator.RemoveRenderWindow(old); }
            }

            this.allocator.Reallocate();

            this.oldwindows = windows;

            foreach (DX11DeviceRenderer rendergraph in this.RenderGraphs.Values)
            {
                rendergraph.Reset();
            }

            
        }

        public void Render(IDX11ResourceDataRetriever sender, IPluginHost host)
        {
            if (!this.Enabled)
            {
                sender.AssignedContext = null;
                return;
            }

            this.gb.Flush();

            //TODO : Allocate new device after
            if (this.RenderGraphs.Count > 0)
            {
                DX11RenderContext onDevice = this.RenderGraphs.Keys.First();
                this.RenderGraphs[onDevice].Render(sender, host);
            }
            else
            {
                sender.AssignedContext = null;
            }
            
   
        }

        public void Render()
        {
            if (!this.Enabled)
            {
                return;
            }

            foreach (DX11RenderContext dev in this.RenderGraphs.Keys)
            {
                this.RenderGraphs[dev].Render(this.FindRenderWindows(dev));
                this.RenderGraphs[dev].EndFrame();
            }
        }

        /// <summary>
        /// Presents all render windows (regardless of device)
        /// </summary>
        public void Present()
        {
            if (!this.Enabled)
            {
                return;
            }

            foreach (IDX11RenderWindow window in this.FindRenderWindows())
            {
                try
                {
                    window.Present();
                }
                catch (Exception ex)
                {
                    this.logger.Log(LogType.Error, "Failed to present render window");
                    this.logger.Log(ex);
                }
            }

            foreach (DX11RenderContext ctx in this.RenderGraphs.Keys)
            {
                ctx.EndFrame();
            }
        }

        #region Find Renderers
        private List<DX11Node> FindRenderWindows(DX11RenderContext device)
        {
            List<DX11Node> renderers = new List<DX11Node>();

            foreach (DX11Node n in this.graph.Nodes)
            {
                if (n.IsAssignable<IDX11RenderWindow>())
                {
                    IDX11RenderWindow window = n.Instance<IDX11RenderWindow>();
                    if (window.RenderContext == device && window.IsVisible)
                    {
                        renderers.Add(n);
                    }
                }
            }
            return renderers;
        }

        private List<IDX11RenderWindow> FindRenderWindows()
        {
            List<IDX11RenderWindow> renderers = new List<IDX11RenderWindow>();

            foreach (DX11Node n in this.graph.Nodes)
            {
                if (n.IsAssignable<IDX11RenderWindow>())
                {
                    IDX11RenderWindow window = n.Instance<IDX11RenderWindow>();
                    //We only care about the window in case it's visible

                    if (window.IsVisible)
                    {
                        renderers.Add(window);
                    }
                }
            }
            return renderers;
        }

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



    }
}
