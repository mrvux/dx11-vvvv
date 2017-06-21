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
using FeralTic.DX11.Utils;
using FeralTic.DX11.Resources.Misc;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11", Version = "StreamOut", Author = "vux", AutoEvaluate = false)]
    public class DX11SORendererNode : IPluginEvaluate, IDX11RendererHost, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        [Input("Layer", Order = 1)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Vertex Size", Order = 8, DefaultValue = 12)]
        protected IDiffSpread<int> FInVSize;

        [Input("Element Count", Order = 8, DefaultValue = 512)]
        protected IDiffSpread<int> FInElemCount;

        [Input("Output Draw Mode", DefaultEnumEntry ="Auto", Order = 1000)]
        protected IDiffSpread<StreamOutputBufferWithRawSupport.OutputDrawMode> FinOutputDrawMode;

        [Input("Output Layout", Order = 10005, CheckIfChanged = true)]
        protected Pin<InputElement> FInLayout;

        [Input("Enabled", DefaultValue = 1, Order = 15)]
        protected ISpread<bool> FInEnabled;

        [Input("View", Order = 16)]
        protected IDiffSpread<Matrix> FInView;

        [Input("Projection", Order = 17)]
        protected IDiffSpread<Matrix> FInProjection;

        [Input("Keep In Memory", Order = 18, Visibility = PinVisibility.OnlyInspector, IsSingle=true)]
        protected ISpread<bool> FInKeepInMemory;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<IDX11Geometry>> FOutGeom;

        [Output("Buffer Out")]
        protected ISpread<DX11Resource<DX11RawBuffer>> FOutBuffer;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();

        public event DX11QueryableDelegate BeginQuery;
        public event DX11QueryableDelegate EndQuery;

        private DX11RenderSettings settings = new DX11RenderSettings();
        private StreamOutputBufferWithRawSupport buffer;
        private bool invalidate = false;

        [ImportingConstructor()]
        public DX11SORendererNode(IPluginHost FHost)
        {

        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }


            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            invalidate = this.FInVSize.IsChanged || this.FInElemCount.IsChanged || this.FInLayout.IsChanged || FinOutputDrawMode.IsChanged;

            if (SpreadMax == 0)
            {
                if (this.buffer != null)
                {
                    this.buffer.Dispose();
                    this.buffer = null;
                }
                return;
            }

            if (this.FOutBuffer[0] == null)
            {
                this.FOutGeom[0] = new DX11Resource<IDX11Geometry>();
                this.FOutBuffer[0] = new DX11Resource<DX11RawBuffer>();
            }
        }

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }

        public void Render(DX11RenderContext context)
        {
            if (this.FOutBuffer.SliceCount == 0)
                return;

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

                ctx.StreamOutput.SetTargets(new StreamOutputBufferBinding(this.buffer.D3DBuffer, 0));

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
                    settings.BackBuffer = null;

                    this.FInLayer.RenderAll(context, settings);
                }

                ctx.StreamOutput.SetTargets(null);

                if (this.EndQuery != null)
                {
                    this.EndQuery(context);
                }

            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FOutBuffer.SliceCount == 0)
                return;

            if (this.updateddevices.Contains(context)) { return; }
            if (this.invalidate || this.buffer == null)
            {
                this.DisposeBuffers(context);

                this.buffer = new StreamOutputBufferWithRawSupport(context, this.FInVSize[0], this.FInElemCount[0],this.FinOutputDrawMode[0],false, this.FInLayout.ToArray());

                this.FOutGeom[0][context] = this.buffer.VertexGeometry;

                if (context.ComputeShaderSupport)
                {
                    this.FOutBuffer[0][context] = this.buffer.RawBuffer;
                }
                else
                {
                    this.FOutBuffer[0][context] = null;
                }
            }

            this.updateddevices.Add(context);
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (force || this.FInKeepInMemory[0] == false)
            {
                this.DisposeBuffers(context);
            }
        }

        #region Dispose Buffers
        private void DisposeBuffers(DX11RenderContext context)
        {
            if (this.buffer != null)
            {
                this.buffer.Dispose();
                this.buffer = null;
            }
        }
        #endregion

        public void Dispose()
        {
            if (this.buffer != null)
            {
                this.buffer.Dispose();
                this.buffer = null;
            }
        }
    }


}
