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

using VVVV.DX11.Interfaces;
using VVVV.DX11.Internals;
using VVVV.DX11.Internals.Helpers;
using VVVV.DX11.Lib.Resources;
using VVVV.DX11.Lib.Resources.Textures.Cube;
using VVVV.DX11.Lib.Devices;
using VVVV.DX11.Lib.Rendering;
using VVVV.DX11.Interfaces.Types;
using FeralTic.Rendering;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11",Version="CubeTexture", Author = "vux")]
    public class DX11CubeRendererNode : IDX11RendererProvider, IPluginEvaluate, IDisposable
    {
        protected IPluginHost FHost;


        #region Input Pins
        [Input("Layer", Order = 1,IsSingle=true)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Clear", DefaultValue = 1, Order = 6)]
        protected ISpread<bool> FInClear;

        [Input("Clear Depth", DefaultValue = 1, Order = 6)]
        protected ISpread<bool> FInClearDepth;


        [Input("Position", Order = 7)]
        protected ISpread<Vector3> FInPosition;

        [Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 1 }, Order = 7)]
        protected ISpread<Color4> FInBgColor;

        [Input("AA Samples per Pixel", DefaultValue = 1, MinValue = 1, Order = 7)]
        protected IDiffSpread<int> FInAASamplesPerPixel;

        [Input("AA Quality", Order = 8)]
        protected IDiffSpread<int> FInAAQuality;

        [Input("Enabled", DefaultValue = 1, Order = 9)]
        protected ISpread<bool> FInEnabled;

        [Input("Enable Depth Buffer", Order = 9)]
        protected IDiffSpread<bool> FInDepthBuffer;

        protected IDiffSpread<EnumEntry> FInFormat;

        [Input("Generate Mip Maps", Order = 4)]
        protected IDiffSpread<bool> FInDoMipMaps;

        [Input("Mip Map Levels", Order = 5)]
        protected IDiffSpread<int> FInMipLevel;

        [Input("Size", Order = 8, DefaultValue=256)]
        protected IDiffSpread<int> FInTextureSize;

        [Input("Projection",Order=9)]
        protected IDiffSpread<Matrix> FInProjection;
        #endregion

        #region Output Pins
        [Output("Cube Size")]
        protected ISpread<int> FOutBufferSize;

        [Output("Texture Out")]
        protected ISpread<DX11Resource<DX11CubeRenderTarget>> FOutTexture;
        #endregion

        private bool genmipmap;
        private int mipmaplevel;
        private bool resetbuffers;
        private int size;
        private SampleDescription sd = new SampleDescription(1, 0);

        private Dictionary<Device, DX11GraphicsRenderer> renderers = new Dictionary<Device, DX11GraphicsRenderer>();
        private List<Device> updateddevices = new List<Device>();
        private List<Device> rendereddevices = new List<Device>();
        private DepthBufferManager depthmanager;


        private List<Vector3> lookats = new List<Vector3>();
        private List<Vector3> upvectors = new List<Vector3>();

        #region Constructor
        [ImportingConstructor()]
        public DX11CubeRendererNode(IPluginHost FHost, IIOFactory iofactory)
        {
            string ename = DX11NullDevice.NullDeviceFormats.GetEnumName(FormatSupport.RenderTarget);

            InputAttribute tattr = new InputAttribute("Target Format");
            tattr.EnumName = ename;
            tattr.DefaultEnumEntry = "R8G8B8A8_UNorm";

            this.FInFormat = iofactory.CreateDiffSpread<EnumEntry>(tattr);

            this.depthmanager = new DepthBufferManager(FHost,iofactory);

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
        public void Evaluate(int SpreadMax)
        {
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
                if (this.FOutTexture[0] != null) { this.FOutTexture[0].Dispose(); }

                this.FOutTexture[0] = new DX11Resource<DX11CubeRenderTarget>();

                this.size = this.FInTextureSize[0];
                this.sd.Count = this.FInAASamplesPerPixel[0];
                this.sd.Quality = this.FInAAQuality[0];
                this.genmipmap = this.FInDoMipMaps[0];
                this.mipmaplevel = Math.Max(FInMipLevel[0], 0);

                this.resetbuffers = true;
                this.depthmanager.NeedReset = true;
            }

            this.FOutBufferSize[0] = this.FInTextureSize[0];
        }
        #endregion

        public void Update(IPluginIO pin, DX11RenderContext OnDevice)
        {
            Device device = OnDevice.Device;

            if (this.updateddevices.Contains(device)) { return; }

            if (!this.renderers.ContainsKey(device))
            {
                this.renderers.Add(device, new DX11GraphicsRenderer(this.FHost, OnDevice));
            }

            if (this.resetbuffers || !this.FOutTexture[0].Contains(device))
            {
                this.FOutTexture[0].Dispose(device);

                this.FOutTexture[0][device] = new DX11CubeRenderTarget(device, this.size, this.sd, DeviceFormatHelper.GetFormat(this.FInFormat[0].Name), this.genmipmap, this.mipmaplevel);
            }

            //Update depth manager
            this.depthmanager.Update(device, this.size, this.size, this.sd);

            this.updateddevices.Add(device);
        }

        #region Render
        public void Render(DX11RenderContext OnDevice)
        {
            Device device = OnDevice.Device;

            //Just in case
            if (!this.updateddevices.Contains(device))
            {
                this.Update(null, OnDevice);
            }

            if (this.rendereddevices.Contains(OnDevice.Device)) { return; }

            if (this.FInEnabled[0])
            {
                DX11GraphicsRenderer renderer = this.renderers[device];

                renderer.EnableDepth = this.FInDepthBuffer[0];
                renderer.DSV = this.depthmanager.GetDSV(OnDevice.Device);



                for (int i = 0; i < 6; i++)
                {
                    this.FOutTexture[0][device].FaceIndex = i;

                    renderer.SetRenderTargets(this.FOutTexture[0][OnDevice.Device]);
                    renderer.SetTargets();

                    if (this.FInClearDepth[0] && this.FInDepthBuffer[0])
                    {
                        this.depthmanager.Clear(device, OnDevice.CurrentDeviceContext);
                    }

                    if (this.FInClear[i])
                    {
                        renderer.Clear(this.FInBgColor[i]);
                    }


                    RenderSettings settings = new RenderSettings();
                    settings.View = Matrix.LookAtLH(this.FInPosition[0], this.lookats[i] + this.FInPosition[0], this.upvectors[i]);
                    settings.Projection = this.FInProjection[0];
                    settings.RenderWidth = this.size;
                    settings.RenderHeight = this.size;

                    float cw = (float)this.size;
                    float ch = (float)this.size;

                    renderer.ViewPortManager.SetDefaultViewPort(cw, ch);
                    renderer.ViewPortManager.ClearScissor();

                    //Call render on all layers
                    for (int j = 0; j < this.FInLayer.SliceCount; j++)
                    {
                        this.FInLayer[j][device].Render(this.FInLayer.PluginIO, OnDevice, settings);
                    }


                    this.rendereddevices.Add(OnDevice.Device);
                }
            }


        }
        #endregion

        public void Destroy(IPluginIO pin, DX11RenderContext OnDevice, bool force)
        {
            if (this.renderers.ContainsKey(OnDevice.Device))
            {
                this.renderers.Remove(OnDevice.Device);
            }

            this.depthmanager.Destroy(OnDevice.Device);
        }

        #region On Dispose
        public void Dispose()
        {
            this.depthmanager.Dispose();

            if (this.FOutTexture[0] != null) { this.FOutTexture[0].Dispose(); }
        }
        #endregion

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }
    }
}
