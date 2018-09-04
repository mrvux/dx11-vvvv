using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using SlimDX;
using VVVV.Utils.VMath;

using SlimDX.Direct3D11;
using System.ComponentModel.Composition;
using FeralTic.DX11;
using FeralTic.DX11.Queries;
using FeralTic.DX11.Resources;
using VVVV.DX11.Lib;
using VVVV.DX11.Internals.Helpers;

namespace VVVV.DX11.Nodes.Renderers.Graphics
{
    [PluginInfo(Name = "Renderer", Category = "DX11", Version = "Volume", Author = "vux", AutoEvaluate = false)]
    public class DX11VolumeRendererNode : IPluginEvaluate, IDX11RendererHost, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        [Input("Layer", Order = 1)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Texture Size", Order = 5, DefaultValues = new double[] { 32, 32,8 })]
        protected IDiffSpread<Vector3D> FInTextureSize;

        [Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 1 }, Order = 7)]
        protected ISpread<Color4> FInBgColor;

        [Input("Clear", DefaultValue = 1, Order = 8)]
        protected ISpread<bool> FInClear;

        [Input("Enabled", DefaultValue = 1, Order = 9)]
        protected ISpread<bool> FInEnabled;

        [Input("Bind As Target", DefaultValue = 1, Order = 10, Visibility= PinVisibility.OnlyInspector)]
        protected ISpread<bool> FInBindTarget;

        [Input("View", Order = 10)]
        protected IDiffSpread<Matrix> FInView;

        [Input("Projection", Order = 11)]
        protected IDiffSpread<Matrix> FInProjection;

        [Output("Buffers", IsSingle = true)]
        protected ISpread<DX11Resource<DX11RenderTexture3D>> FOutBuffers;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        IDiffSpread<EnumEntry> FInFormat;

        protected int width;
        protected int height;
        protected int depth;

        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        private bool reset = false;

        [ImportingConstructor()]
        public DX11VolumeRendererNode(IPluginHost FHost,IIOFactory factory)
        {
            string ename = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.RenderTarget);

            InputAttribute tattr = new InputAttribute("Target Format");
            tattr.EnumName = ename;
            tattr.DefaultEnumEntry = "R8G8B8A8_UNorm";
            tattr.CheckIfChanged = true;

            this.FInFormat = factory.CreateDiffSpread<EnumEntry>(tattr);
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }

            reset = this.FInTextureSize.IsChanged || this.FInFormat.IsChanged;

            if (reset)
            {
                this.FOutBuffers.SafeDisposeAll();
            }

            if (this.FOutBuffers[0] == null)
            {
                this.FOutBuffers[0] = new DX11Resource<DX11RenderTexture3D>();
            }

            if (reset)
            {
                Vector3D v = this.FInTextureSize[0];

                width = (int)v.x;
                height = (int)v.y;
                depth = (int)v.z;
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

            if (!this.FInLayer.IsConnected) { return; }

            if (this.FInEnabled[0])
            {
                if (this.BeginQuery != null)
                {
                    this.BeginQuery(context);
                }

                

                if (this.FInClear[0])
                {
                    ctx.ClearRenderTargetView(this.FOutBuffers[0][context].RTV, this.FInBgColor[0]);
                }

                /*Viewport vp = new Viewport(0, 0, this.width, this.height);
                ctx.Rasterizer.SetViewports(vp);*/
                //ctx.OutputMerger.SetTargets(this.FOutBuffers[0][context].RTV);

                int rtmax = Math.Max(this.FInProjection.SliceCount, this.FInView.SliceCount);

                if (this.FInBindTarget[0])
                {
                    context.RenderTargetStack.Push(this.FOutBuffers[0][context]);
                }

                for (int i = 0; i < rtmax; i++)
                {
                    DX11RenderSettings settings = new DX11RenderSettings();
                    settings.ViewportIndex = 0;
                    settings.ViewportCount = 1;
                    settings.View = this.FInView[i];
                    settings.Projection = this.FInProjection[i];
                    settings.ViewProjection = settings.View * settings.Projection;
                    settings.RenderWidth = this.width;
                    settings.RenderHeight = this.height;
                    settings.RenderDepth = this.depth;
                    settings.BackBuffer = this.FOutBuffers[0][context];
                    settings.CustomSemantics.Clear();
                    settings.ResourceSemantics.Clear();

                    this.FInLayer.RenderAll(context, settings);
                }

                if (this.FInBindTarget[0])
                {
                    context.RenderTargetStack.Pop();
                }

                if (this.EndQuery != null)
                {
                    this.EndQuery(context);
                }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (reset || !this.FOutBuffers[0].Contains(context))
            {
                this.DisposeBuffers(context);

                DX11RenderTexture3D rt = new DX11RenderTexture3D(context, width, height, depth, DeviceFormatHelper.GetFormat(this.FInFormat[0].Name));

                this.FOutBuffers[0][context] = rt;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.DisposeBuffers(context);    
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
