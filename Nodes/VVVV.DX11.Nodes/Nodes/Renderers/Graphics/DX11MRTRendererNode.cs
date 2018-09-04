using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using VVVV.DX11.Lib;
using VVVV.DX11.Internals.Helpers;

namespace VVVV.DX11
{
    [PluginInfo(Name = "Renderer", Category = "DX11", Version = "MRT", Author = "vux,tonfilm", AutoEvaluate = false)]
    public class DX11MRTRendererNode : AbstractDX11Renderer2DNode
    {
        #region Inputs
        [Input("Target Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInTargetCount;

        IDiffSpread<EnumEntry> FInFormat;

        [Input("Generate Mip Maps", Order = 4)]
        protected IDiffSpread<bool> FInDoMipMaps;

        [Input("Mip Map Levels", Order = 5)]
        protected IDiffSpread<int> FInMipLevel;

        [Input("Texture Size", Order = 8, DefaultValues = new double[] { 400, 300 })]
        protected IDiffSpread<Vector2D> FInTextureSize;
        #endregion


        #region Output Pins
        [Output("Buffer Size")]
        protected ISpread<Vector2D> FOutBufferSize;

        [Output("Buffers")]
        protected ISpread<DX11Resource<DX11RenderTarget2D>> FOutBuffers;
        #endregion

        private int buffercount;
        private bool genmipmap;
        private int mipmaplevel;

        private bool resetbuffers;


        #region Constructor
        [ImportingConstructor()]
        public DX11MRTRendererNode(IPluginHost FHost,IIOFactory iofactory)
        {
            string ename = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.RenderTarget);

            InputAttribute tattr = new InputAttribute("Target Format");
            tattr.EnumName = ename;
            tattr.DefaultEnumEntry = "R8G8B8A8_UNorm";

            this.FInFormat = iofactory.CreateDiffSpread<EnumEntry>(tattr);

            this.depthmanager = new DepthBufferManager(FHost,iofactory);
        }
        #endregion

        #region Evaluate
        protected override void OnEvaluate(int SpreadMax)
        {
            this.resetbuffers = false;

            if (this.FInTextureSize.IsChanged || this.FInTargetCount.IsChanged
                || this.FInFormat.IsChanged 
                || this.FInAASamplesPerPixel.IsChanged
                || this.FInDoMipMaps.IsChanged
                || this.FInMipLevel.IsChanged)
            {
                this.FOutBuffers.SafeDisposeAll();
                this.FOutBuffers.SliceCount = this.FInTargetCount[0];
                for (int i = 0; i < this.FOutBuffers.SliceCount; i++)
                {
                    this.FOutBuffers[i] =new DX11Resource<DX11RenderTarget2D>();
                }


                this.width = Convert.ToInt32(this.FInTextureSize[0].x);
                this.height = Convert.ToInt32(this.FInTextureSize[0].y);
                this.buffercount = this.FInTargetCount[0];
                this.sd.Count = Convert.ToInt32(this.FInAASamplesPerPixel[0].Name);
                this.sd.Quality = 0;
                this.genmipmap = this.FInDoMipMaps[0];
                this.mipmaplevel = Math.Max(FInMipLevel[0], 0);

                this.resetbuffers = true;
                this.depthmanager.NeedReset = true;
            }

            this.FOutBufferSize[0] = new Vector2D(this.width, this.height);
        }
        #endregion

        #region OnUpdate
        protected override void OnUpdate(DX11RenderContext context)
        {
            if (this.resetbuffers || !this.FOutBuffers[0].Contains(context))
            {
                this.FOutBuffers.SafeDisposeAll(context);

                for (int i = 0; i < this.FInTargetCount[0]; i++)
                {
                    DX11RenderTarget2D rt = new DX11RenderTarget2D(context, this.width, this.height,
                    this.sd, DeviceFormatHelper.GetFormat(this.FInFormat[i].Name), this.genmipmap, this.mipmaplevel);

                    #if DEBUG
                    rt.Resource.DebugName = "MRTRenderTexture";
                    #endif

                    this.FOutBuffers[i][context] = rt;
                }
            }
        }
        #endregion

        #region Destroy
        protected override void OnDestroy(DX11RenderContext context, bool force)
        {
            this.FOutBuffers.SafeDisposeAll(context); 
        }
        #endregion

        protected override void DoClear(DX11RenderContext context)
        {
            for (int i = 0; i < this.FOutBuffers.SliceCount; i++)
            {
                if (this.FInClear[i])
                {
                    context.CurrentDeviceContext.ClearRenderTargetView(this.FOutBuffers[i][context].RTV, this.FInBgColor[i]);
                }
            }
        }

        #region Before Render
        protected override void BeforeRender(DX11GraphicsRenderer renderer, DX11RenderContext context)
        {
            IDX11RenderTargetView[] rtvs = new IDX11RenderTargetView[this.FOutBuffers.SliceCount];
            for (int i = 0; i < this.FOutBuffers.SliceCount;i++)
            {
                rtvs[i] = this.FOutBuffers[i][context];
            }

            renderer.EnableDepth = this.FInDepthBuffer[0];
            renderer.DepthStencil = this.depthmanager.GetDepthStencil(context);
            renderer.DepthMode = this.depthmanager.Mode;
            renderer.SetRenderTargets(rtvs);
            
        }
        #endregion

        #region After Render
        protected override void AfterRender(DX11GraphicsRenderer renderer, DX11RenderContext context)
        {
            if (this.genmipmap && this.sd.Count == 1)
            {
                for (int i = 0; i < this.FOutBuffers.SliceCount; i++)
                {
                    context.CurrentDeviceContext.GenerateMips(this.FOutBuffers[i][context].SRV);
                }
            }
        }
        #endregion

        #region On Dispose
        protected override void OnDispose()
        {
            this.FOutBuffers.SafeDisposeAll();
        }
        #endregion
    }
}
