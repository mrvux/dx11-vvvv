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
using VVVV.DX11;
using VVVV.DX11.Lib.Rendering;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11", Version = "RawBuffer Advanced", Author = "microdee", AutoEvaluate = false)]
    public class DX11MultiRawBufferRendererNode : IPluginEvaluate, IDX11RendererProvider, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        [Input("Layer", Order = 1)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Size", Order = 8, DefaultValue = 512)]
        protected IDiffSpread<int> FInSize;

        [Input("Semantic", Order = 9, DefaultString = "UAV")]
        protected IDiffSpread<string> FSemantic;
        [Input("Bind SRV", Order = 10)]
        protected IDiffSpread<bool> FBindSRV;
        [Input("SRV Semantic", Order = 11, DefaultString = "SRV")]
        protected IDiffSpread<string> FSRVSemantic;

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

        [Output("Buffers")]
        protected ISpread<DX11Resource<DX11RawBuffer>> FOutBuffers;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        protected List<int> sizes = new List<int>();
        protected List<string> semantics = new List<string>();
        protected List<IDX11RenderSemantic> rsemantics = new List<IDX11RenderSemantic>();
        private List<DX11RawBufferFlags> flags = new List<DX11RawBufferFlags>();

        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();

        private bool reset = false;


        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        private DX11RenderSettings settings = new DX11RenderSettings();

        [ImportingConstructor()]
        public DX11MultiRawBufferRendererNode(IPluginHost FHost)
        {

        }

        public void Evaluate(int SpreadMax)
        {
            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            FOutBuffers.SliceCount = FSemantic.SliceCount;

            reset = this.FInSize.IsChanged || this.FInVBO.IsChanged || this.FInIBO.IsChanged || this.FInABO.IsChanged || this.FSemantic.IsChanged;

            for (int i = 0; i < FOutBuffers.SliceCount; i++)
            {
                if (this.FOutBuffers[i] == null)
                {
                    this.FOutBuffers[i] = new DX11Resource<DX11RawBuffer>();
                    reset = true;
                }
            }
            if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }

            for (int i = 0; i < FOutBuffers.SliceCount; i++)
            {
                DX11Resource<DX11RawBuffer> res = this.FOutBuffers[i];
                this.FOutBuffers[i] = res;
            }

            if (reset)
            {
                sizes.Clear();
                semantics.Clear();
                flags.Clear();
                for (int i = 0; i < FSemantic.SliceCount; i++)
                {
                    sizes.Add(FInSize[i]);
                    semantics.Add(FSemantic[i]);

                    DX11RawBufferFlags tflags = new DX11RawBufferFlags();
                    tflags.AllowIndexBuffer = this.FInIBO[i];
                    tflags.AllowVertexBuffer = this.FInVBO[i];
                    tflags.AllowArgumentBuffer = this.FInABO[i];
                    flags.Add(tflags);
                }
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
                this.Update(null, context);
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

                settings.ViewportIndex = 0;
                settings.ViewportCount = 1;
                settings.View = this.FInView[0];
                settings.Projection = this.FInProjection[0];
                settings.ViewProjection = settings.View * settings.Projection;
                settings.RenderWidth = 1;
                settings.RenderHeight = 1;
                settings.RenderDepth = 1;
                settings.BackBuffer = null;
                settings.CustomSemantics = rsemantics;

                for (int i = 0; i < this.FInLayer.SliceCount; i++)
                {
                    this.FInLayer[i][context].Render(this.FInLayer.PluginIO, context, settings);
                }

                if (this.EndQuery != null)
                {
                    this.EndQuery(context);
                }
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (this.updateddevices.Contains(context)) { return; }

            foreach (IDX11RenderSemantic semres in rsemantics)
            {
                semres.Dispose();
            }
            if (reset)
            {
                rsemantics.Clear();
                this.DisposeBuffers(context);

                for (int i = 0; i < FOutBuffers.SliceCount; i++)
                {
                    if (reset || !this.FOutBuffers[i].Contains(context))
                    {
                        DX11RawBuffer rb = new DX11RawBuffer(context.Device, this.sizes[i], this.flags[i]);
                        this.FOutBuffers[i][context] = rb;

                        RWBufferRenderSemantic uavbs = new RWBufferRenderSemantic(FSemantic[i], false);
                        uavbs.Data = this.FOutBuffers[i][context];
                        rsemantics.Add(uavbs);

                        if (FBindSRV[i])
                        {
                            BufferRenderSemantic srvbs = new BufferRenderSemantic(FSRVSemantic[i], false);
                            srvbs.Data = this.FOutBuffers[i][context];
                            rsemantics.Add(srvbs);
                        }
                    }
                }
            }

            this.updateddevices.Add(context);
        }

        public void Destroy(IPluginIO pin, DX11RenderContext OnDevice, bool force)
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
            for (int i = 0; i < this.FOutBuffers.SliceCount; i++)
            {
                if (this.FOutBuffers[i] != null) { this.FOutBuffers[i].Dispose(); }
            }
        }
    }
}
