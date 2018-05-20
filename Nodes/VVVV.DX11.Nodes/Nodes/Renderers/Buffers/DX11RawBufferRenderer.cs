using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using SlimDX;
using SlimDX.Direct3D11;

using FeralTic.DX11.Queries;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11", Version = "Buffer.Raw", Author = "vux", AutoEvaluate = false)]
    public class DX11RawBufferRendererNode : IPluginEvaluate, IDX11RendererHost, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        [Input("Layer", Order = 1)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Size", Order = 8, DefaultValue = 512)]
        protected IDiffSpread<int> FInSize;

        [Input("Allow VertexBuffer", DefaultValue = 0, Order = 12)]
        protected IDiffSpread<bool> FInVBO;

        [Input("Allow IndexBuffer", DefaultValue = 0, Order = 13)]
        protected IDiffSpread<bool> FInIBO;

        [Input("Allow Argument Buffer", DefaultValue = 0, Order = 14)]
        protected IDiffSpread<bool> FInABO;

        [Input("Enabled", DefaultValue = 1, Order = 15)]
        protected ISpread<bool> FInEnabled;

        [Input("View", Order = 16)]
        protected IDiffSpread<Matrix> FInView;

        [Input("Projection", Order = 17)]
        protected IDiffSpread<Matrix> FInProjection;

        [Output("Buffers", IsSingle = true)]
        protected ISpread<DX11Resource<DX11RawBuffer>> FOutBuffers;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        protected int size;
        private DX11RawBufferFlags flags = new DX11RawBufferFlags();

        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();

        private bool reset = false;


        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        private DX11RenderSettings settings = new DX11RenderSettings();

        [ImportingConstructor()]
        public DX11RawBufferRendererNode(IPluginHost FHost)
        {

        }

        public void Evaluate(int SpreadMax)
        {
            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            reset = this.FInSize.IsChanged || this.FInVBO.IsChanged || this.FInIBO.IsChanged || this.FInABO.IsChanged;

            if (this.FOutBuffers[0] == null)
            {
                this.FOutBuffers[0] = new DX11Resource<DX11RawBuffer>();
            }
            if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }

            DX11Resource<DX11RawBuffer> res = this.FOutBuffers[0];
            this.FOutBuffers[0] = res;

            if (reset)
            {
                this.size = this.FInSize[0];
                this.flags.AllowIndexBuffer = this.FInIBO[0];
                this.flags.AllowVertexBuffer = this.FInVBO[0];
                this.flags.AllowArgumentBuffer = this.FInABO[0];
            }
        }

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }

        public void Render(DX11RenderContext context)
        {
            Device device = context.Device;
            DeviceContext ctx = context.CurrentDeviceContext;

            //Just in case
            if (!this.updateddevices.Contains(context))
            {
                this.Update(context);
            }



            if (!this.FInLayer.IsConnected) { return; }

            if (this.rendereddevices.Contains(context)) { return; }

            if (this.FInEnabled[0])
            {
                if (this.BeginQuery != null)
                {
                    this.BeginQuery(context);
                }

                context.CurrentDeviceContext.OutputMerger.SetTargets(new RenderTargetView[0]);

                int rtmax = Math.Max(this.FInProjection.SliceCount, this.FInView.SliceCount);

                for (int i = 0; i < rtmax; i++)
                {
                    settings.ViewportIndex = 0;
                    settings.ViewportCount = 1;
                    settings.View = this.FInView[i];
                    settings.Projection = this.FInProjection[i];
                    settings.ViewProjection = settings.View * settings.Projection;
                    settings.RenderWidth = 1;
                    settings.RenderHeight = 1;
                    settings.RenderDepth = 1;
                    settings.BackBuffer = this.FOutBuffers[0][context];

                    this.FInLayer.RenderAll(context, settings);
                }

                if (this.EndQuery != null)
                {
                    this.EndQuery(context);
                }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.updateddevices.Contains(context)) { return; }
            if (reset || !this.FOutBuffers[0].Contains(context))
            {
                this.DisposeBuffers(context);

                DX11RawBuffer rb = new DX11RawBuffer(context.Device, this.size, this.flags);
                this.FOutBuffers[0][context] = rb;
            }

            this.updateddevices.Add(context);
        }

        public void Destroy(DX11RenderContext OnDevice, bool force)
        {
            //this.DisposeBuffers(OnDevice.Device);
        }

        #region Dispose Buffers
        private void DisposeBuffers(DX11RenderContext context)
        {
            for (int i = 0; i < this.FOutBuffers.SliceCount; i++)
            {
                this.FOutBuffers[i].Dispose(context);
            }
        }
        #endregion

        public void Dispose()
        {
            this.FOutBuffers.SafeDisposeAll();
        }
    }
}
