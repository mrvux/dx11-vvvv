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
using System.Threading.Tasks;

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

        private List<DX11Node> oldwindows = new List<DX11Node>();

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

        public DX11RenderContext PreferredDataContext { get; set; }

        public void Reset()
        {
            
            List<DX11Node> windows = this.FindRenderWindows();
            foreach (DX11Node win in windows)
            {
                this.allocator.AddRenderWindow(win.Interfaces.RenderWindow);
            }

            foreach (DX11Node old in this.oldwindows)
            {
                if (!windows.Contains(old)) { this.allocator.RemoveRenderWindow(old.Interfaces.RenderWindow); }
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

            if (this.RenderGraphs.Count > 0)
            {
                DX11RenderContext dataRenderContext;
                //Check if we have a preffered device for that task (and check that we have render graph for it
                if (this.PreferredDataContext != null && this.RenderGraphs.ContainsKey(this.PreferredDataContext))
                {
                    dataRenderContext = this.PreferredDataContext;
                }
                else
                {
                    dataRenderContext = this.RenderGraphs.Keys.First();
                }


                this.RenderGraphs[dataRenderContext].Render(sender, host);
            }
            else
            {
                sender.AssignedContext = null;
            }
            
   
        }


        public bool AllowThreadPresentation { get; set; }
        public bool AllowThreadPerDevice { get; set; }

        public void Render()
        {
            if (!this.Enabled)
            {
                return;
            }

            if (this.AllowThreadPerDevice)
            {
                List<DX11RenderContext> devices = this.RenderGraphs.Keys.ToList();

                Task[] renderTasks = new Task[devices.Count];
                for (int i = 0; i < devices.Count; i++)
                {
                    int index = i;
                    renderTasks[i] = Task.Run(() => this.RenderDevice(devices[index]));
                }
                Task.WaitAll(renderTasks);
            }
            else
            {
                foreach (DX11RenderContext dev in this.RenderGraphs.Keys)
                {
                    RenderDevice(dev);
                }
            }
        }

        private void RenderDevice(DX11RenderContext dev)
        {
            var renderWindows = this.FindRenderStartPoints(dev);
            this.RenderGraphs[dev].Render(renderWindows);
            this.RenderGraphs[dev].EndFrame();
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

            List<DX11Node> startPoints = this.FindRenderStartPoints();

            if (!this.AllowThreadPresentation)
            {
                foreach (DX11Node window in startPoints)
                {
                    this.PresentWindow(window);
                }
            }
            else
            {
                Task[] presentTasks = new Task[startPoints.Count];
                for (int i = 0; i < presentTasks.Length; i++)
                {
                    int index = i;
                    presentTasks[i] = Task.Run(() => this.PresentWindow(startPoints[index]));
                }
                Task.WaitAll(presentTasks);
            }

            foreach (DX11RenderContext ctx in this.RenderGraphs.Keys)
            {
                ctx.EndFrame();
            }
        }

        private void PresentWindow(DX11Node window)
        {
            try
            {
                window.Interfaces.RenderStartPoint.Present();
            }
            catch (Exception ex)
            {
                this.logger.Log(LogType.Error, "Failed to present render window");
                this.logger.Log(ex);
            }
        }



        private List<DX11Node> FindRenderWindows()
        {
            List<DX11Node> renderers = new List<DX11Node>();
            foreach (DX11Node n in this.graph.RenderWindows)
            {
                IDX11RenderWindow window = n.Interfaces.RenderWindow;
                if (window.Enabled)
                {
                    renderers.Add(n);
                }
            }
            return renderers;
        }

        private List<DX11Node> FindRenderStartPoints()
        {
            List<DX11Node> renderers = new List<DX11Node>();
            foreach (DX11Node n in this.graph.RenderStartPoints)
            {

                IDX11RenderStartPoint window = n.Interfaces.RenderStartPoint;
                if (window.Enabled)
                {
                    renderers.Add(n);
                }
            }
            return renderers;
        }

        private List<DX11Node> FindRenderStartPoints(DX11RenderContext context)
        {
            List<DX11Node> renderers = new List<DX11Node>();
            foreach (DX11Node n in this.graph.RenderStartPoints)
            {

                IDX11RenderStartPoint window = n.Interfaces.RenderStartPoint;
                if (window.Enabled && window.RenderContext == context)
                {
                    renderers.Add(n);
                }
            }
            return renderers;
        }
    }
}
