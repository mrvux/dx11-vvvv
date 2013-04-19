using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Hosting.Pins;

using FeralTic.DX11.Resources;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;
using VVVV.DX11.Internals.Helpers;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11",Version="CubeTexture", Author = "vux")]
    public class DX11CubeRendererNode : AbstractDX11Renderer2DNode
    {
        protected IPluginHost FHost;

        #region Inputs
        IDiffSpread<EnumEntry> FInFormat;

        [Input("Generate Mip Maps", Order = 4)]
        protected IDiffSpread<bool> FInDoMipMaps;

        [Input("Mip Map Levels", Order = 5)]
        protected IDiffSpread<int> FInMipLevel;

        [Input("Texture Size", Order = 8, DefaultValue = 256, IsSingle=true)]
        protected IDiffSpread<int> FInTextureSize;
        #endregion

        #region Output Pins
        [Output("Cube Size")]
        protected ISpread<int> FOutBufferSize;

        [Output("Texture Out",IsSingle=true)]
        protected ISpread<DX11Resource<DX11CubeRenderTarget>> FOutTexture;
        #endregion

        private bool genmipmap;
        private int mipmaplevel;
        private bool resetbuffers;
        private int size;

        private List<Vector3> lookats = new List<Vector3>();
        private List<Vector3> upvectors = new List<Vector3>();

        #region Constructor
        [ImportingConstructor()]
        public DX11CubeRendererNode(IPluginHost FHost, IIOFactory iofactory)
        {
            string ename = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.RenderTarget);

            InputAttribute tattr = new InputAttribute("Target Format");
            tattr.IsSingle = true;
            tattr.EnumName = ename;
            tattr.DefaultEnumEntry = "R8G8B8A8_UNorm";

            this.FInFormat = iofactory.CreateDiffSpread<EnumEntry>(tattr);

            this.depthmanager = new DepthBufferManager(FHost, iofactory);

            this.lookats.Add(new Vector3(1.0f, 0.0f, 0.0f));
            this.lookats.Add(new Vector3(-1.0f, 0.0f, 0.0f));
            this.lookats.Add(new Vector3(0.0f, 1.0f, 0.0f));

            this.lookats.Add(new Vector3(0.0f, - 1.0f, 0.0f));
            this.lookats.Add(new Vector3(0.0f, 0.0f, 1.0f));
            this.lookats.Add(new Vector3(0.0f, 0.0f, -1.0f));

            this.upvectors.Add(new Vector3(0.0f, 1.0f, 0.0f));
            this.upvectors.Add(new Vector3(0.0f, 1.0f, 0.0f));
            this.upvectors.Add(new Vector3(0.0f, 0.0f, -1.0f));
            this.upvectors.Add(new Vector3(0.0f, 0.0f, 1.0f));
            this.upvectors.Add(new Vector3(0.0f, 1.0f, 0.0f));
            this.upvectors.Add(new Vector3(0.0f, 1.0f, 0.0f));
        }
        #endregion

        #region Evaluate
        protected override void OnEvaluate(int SpreadMax)
        {
            if (this.FOutTexture[0] == null) { this.FOutTexture[0] = new DX11Resource<DX11CubeRenderTarget>(); }

            this.depthmanager.NeedReset = false;
            this.resetbuffers = false;
            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            if (this.FInTextureSize.IsChanged
                || this.FInFormat.IsChanged
                || this.FInAAQuality.IsChanged
                || this.FInAASamplesPerPixel.IsChanged
                || this.FInDoMipMaps.IsChanged
                || this.FInMipLevel.IsChanged)
            {
                this.size = this.FInTextureSize[0];
                this.sd.Count = this.FInAASamplesPerPixel[0];
                this.sd.Quality = this.FInAAQuality[0];
                this.genmipmap = this.FInDoMipMaps[0];
                this.mipmaplevel = Math.Max(FInMipLevel[0], 0);
                this.width = this.size;
                this.height = this.size;

                this.resetbuffers = true;
                this.depthmanager.NeedReset = true;
            }

            this.FOutBufferSize[0] = this.FInTextureSize[0];
        }
        #endregion

        #region OnUpdate
        protected override void OnUpdate(DX11RenderContext context)
        {
            if (this.resetbuffers || !this.FOutTexture[0].Contains(context))
            {
                this.DisposeBuffers(context);

                DX11CubeRenderTarget rt = new DX11CubeRenderTarget(context, this.size, new SampleDescription(1, 0),
                    DeviceFormatHelper.GetFormat(this.FInFormat[0].Name), this.genmipmap, this.mipmaplevel);

                this.FOutTexture[0][context] = rt;
            }
        }
        #endregion

        #region Dispose Buffers
        private void DisposeBuffers(DX11RenderContext context)
        {
                this.FOutTexture[0].Dispose(context);
        }
        #endregion

        #region Destroy
        protected override void OnDestroy(DX11RenderContext context, bool force)
        {
            this.DisposeBuffers(context);
        }
        #endregion


        protected override void BeforeRender(DX11GraphicsRenderer renderer, DX11RenderContext OnDevice)
        {
            
        }

        protected override void AfterRender(DX11GraphicsRenderer renderer, DX11RenderContext OnDevice)
        {
            
        }

        protected override void OnDispose()
        {
            this.depthmanager.Dispose();

            if (this.FOutTexture[0] != null) { this.FOutTexture[0].Dispose(); }          
        }
    }
}
