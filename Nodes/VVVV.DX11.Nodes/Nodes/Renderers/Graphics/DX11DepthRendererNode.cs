using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins.Config;
using SlimDX.Direct3D11;
using VVVV.Hosting.Pins;
using SlimDX.DXGI;

using Device = SlimDX.Direct3D11.Device;
using VVVV.DX11.Internals.Helpers;


namespace VVVV.DX11
{
    [PluginInfo(Name = "Renderer", Category = "DX11", Version = "TempTarget", Author = "vux,tonfilm", AutoEvaluate = false)]
    public class DX11TempRTRendererNode : AbstractDX11Renderer2DNode, IPluginEvaluate, IDisposable
    {
        #region Inputs
        [Input("Generate Mip Maps", Order = 4)]
        IDiffSpread<bool> FInDoMipMaps;

        [Input("Mip Map Levels", Order = 5)]
        IDiffSpread<int> FInMipLevel;

        [Input("Texture Size", Order = 8, DefaultValues = new double[] { 400, 300 })]
        IDiffSpread<Vector2D> FInTextureSize;
        #endregion

        #region Config Pins
        //IDiffSpread<EnumEntry> FCfgDepthBufferFormat;
        #endregion

        #region Output Pins
        [Output("Buffer Size")]
        ISpread<Vector2D> FOutBufferSize;

        [Output("Buffers")]
        ISpread<DX11Texture2D> FOutBuffers;

        #endregion

        private bool FInvalidateDepth = false;

        private SampleDescription sampledesc = new SampleDescription(1, 0);

        #region Constructor
        [ImportingConstructor()]
        public DX11TempRTRendererNode(IPluginHost FHost)
        {
            string ename = VDX11.NullDeviceFormats.GetEnumName(FormatSupport.RenderTarget);

            InputAttribute tattr = new InputAttribute("Target Format");
            tattr.EnumName = ename;
            tattr.DefaultEnumEntry = "R8G8B8A8_UNorm";

            this.FInFormat = PinFactory.CreateDiffSpread<EnumEntry>(FHost, tattr);


            this.FDepthManager = new DepthBufferManager(FHost);
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FInTextureSize.IsChanged
                || this.FInFormat.IsChanged || this.FInAAQuality.IsChanged
                || this.FInAASamplesPerPixel.IsChanged
                || this.FInDoMipMaps.IsChanged
                || this.FInMipLevel.IsChanged)
            {
                //this.FOutResBuffers.SliceCount = this.FInTargetCount[0];
                this.FOutBuffers.SliceCount = 1;
                this.FOutBuffers[0] = new DX11Texture2D();
                this.width = Convert.ToInt32(this.FInTextureSize[0].x);
                this.height = Convert.ToInt32(this.FInTextureSize[0].y);
                this.sampledesc.Count = this.FInAASamplesPerPixel[0];
                this.sampledesc.Quality = this.FInAAQuality[0];
                this.genmipmap = this.FInDoMipMaps[0];
                this.mipmaplevel = Math.Max(FInMipLevel[0], 0);
                this.FInvalidateDepth = true;
            }


            this.FOutBufferSize[0] = new Vector2D(this.width, this.height);
        }
        #endregion


        #region Dispose
        public void Dispose()
        {

        }
        #endregion


        #region Update
        protected override void Update()
        {
            if (this.FInvalidateDepth || this.FDepthManager.NeedReset)
            {
                this.FDepthManager.Reset(VDX11.Device, this.width, this.height, this.sampledesc);
                this.FInvalidateDepth = false;
            }
        }

        #region Pre/Post Render
        protected override void PreRender()
        {

            target = TexturePoolManager.GetPool(VDX11.Device).GetTempRenderTarget(
                this.width, this.height, DeviceFormatHelper.GetFormat(this.FInFormat[0]), new SampleDescription(1, 0), this.FInDoMipMaps[0], this.FInMipLevel[0]);

            if (this.FInDepthBuffer[0])
            {
                this.SetTargets(this.FDepthManager.DSV, target.RTV);
            }
            else
            {
                this.SetTargets(null, target.RTV);
            }


        }
        #endregion

        protected override DX11Texture2D GetLastBuffer()
        {
            return this.lastbuffer;
        }

        protected override void PostRender()
        {
            if (this.genmipmap && this.sampledesc.Count == 1)
            {
                VDX11.Device.ImmediateContext.GenerateMips(target.SRV);
            }


            if (this.FInSaveLastBuffer[0])
            {
                if (this.lastbuffer != null)
                {
                    if (this.lastbuffer.Resource.Description != target.Resource.Description)
                    {
                        this.lastbuffer.Dispose();
                        this.lastbuffer = null;
                    }
                }

                if (this.lastbuffer == null)
                {
                    Texture2D t = new Texture2D(VDX11.Device, target.Resource.Description);
                    ShaderResourceView srv = new ShaderResourceView(VDX11.Device, t);
                    this.lastbuffer = DX11Texture2D.FromTextureAndSRV(t, srv);
                }

                VDX11.Device.ImmediateContext.CopyResource(target.Resource, this.lastbuffer.Resource);
            }
            else
            {
                if (this.lastbuffer != null) { this.lastbuffer.Dispose(); this.lastbuffer = null; }
            }

            this.FOutBuffers[0].AssignResourceAndSRV(target.Resource, target.SRV);
        }
        #endregion

        #region SwapChain/Present
        public override bool HasSwapChain
        {
            get { return false; }
        }

        public override void Present()
        {

        }
        #endregion
    }
}
