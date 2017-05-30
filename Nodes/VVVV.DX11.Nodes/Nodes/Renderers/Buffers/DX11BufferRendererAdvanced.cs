using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using SlimDX;
using SlimDX.Direct3D11;
using System.ComponentModel.Composition;

using FeralTic.DX11.Queries;
using FeralTic.DX11.Resources;
using FeralTic.DX11;
using System.IO;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11", Version = "Buffer Advanced", Author = "vux", AutoEvaluate = false)]
    public class DX11AdvancedBufferRendererNode : IPluginEvaluate, IDX11RendererHost, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        [Input("Layer", Order = 1)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Element Count", Order = 8, DefaultValue = 512)]
        protected IDiffSpread<int> FInElementCount;

        [Input("Stride", Order = 8, DefaultValue = 16)]
        protected IDiffSpread<int> FInStride;

        [Input("Buffer Mode", Order = 8, DefaultValue = 0)]
        protected IDiffSpread<eDX11BufferMode> FInMode;

        [Input("Reset Counter",IsBang=true )]
        protected IDiffSpread<bool> FInResetCounter;

        [Input("Reset Counter Value")]
        protected IDiffSpread<int> FInResetCounterValue;

        [Input("Enabled", DefaultValue = 1, Order = 15)]
        protected ISpread<bool> FInEnabled;

        [Input("View", Order = 16)]
        protected IDiffSpread<Matrix> FInView;

        [Input("Projection", Order = 17)]
        protected IDiffSpread<Matrix> FInProjection;

        [Output("Buffers", IsSingle = true)]
        protected ISpread<DX11Resource<IDX11RWStructureBuffer>> FOutBuffers;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        protected int cnt;
        protected int stride;

        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();

        private bool reset = false;


        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        private DX11RenderSettings settings = new DX11RenderSettings();

        [ImportingConstructor()]
        public DX11AdvancedBufferRendererNode(IPluginHost FHost)
        {

        }

        public void Evaluate(int SpreadMax)
        {
            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            reset = this.FInElementCount.IsChanged || this.FInStride.IsChanged || this.FInMode.IsChanged;

            if (this.FOutBuffers[0] == null)
            {
                this.FOutBuffers[0] = new DX11Resource<IDX11RWStructureBuffer>();
            }
            if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }

            DX11Resource<IDX11RWStructureBuffer> res = this.FOutBuffers[0];
            this.FOutBuffers[0] = res;

            if (reset)
            {
                this.stride = this.FInStride[0];
                this.cnt = this.FInElementCount[0];
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



            if (!this.FInLayer.PluginIO.IsConnected) { return; }

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
                    settings.RenderWidth = this.cnt;
                    settings.RenderHeight = this.cnt;
                    settings.RenderDepth = this.cnt;
                    settings.BackBuffer = this.FOutBuffers[0][context];
                    

                    if (this.FInResetCounter[0])
                    {
                        settings.ResetCounter = true;
                        settings.CounterValue = this.FInResetCounterValue[0];
                    }
                    else
                    {
                        settings.ResetCounter = false;
                    }

                    // this.rwbuffersemantic.Data = this.FOutBuffers[0][context];

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

                eDX11BufferMode mode = this.FInMode[0];

                DX11RWStructuredBuffer rt = new DX11RWStructuredBuffer(context.Device, this.cnt, this.stride, mode);

                this.FOutBuffers[0][context] = rt;
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
