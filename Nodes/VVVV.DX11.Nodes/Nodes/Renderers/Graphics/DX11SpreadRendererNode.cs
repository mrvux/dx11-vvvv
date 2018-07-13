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
using FeralTic.DX11.Resources;
using FeralTic.DX11;
using FeralTic.DX11.Queries;
using VVVV.DX11.Lib;
using VVVV.DX11.Internals.Helpers;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11", Version = "TextureSpread", Author = "vux")]
    public class DX11TextureSpreadRendererNode : IDX11RendererHost, IPluginEvaluate, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        IDiffSpread<EnumEntry> FInFormat;
        private IDiffSpread<EnumEntry> depthformatpin;

        [Input("Layer", Order = 1)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Size", DefaultValues = new double[] { 512, 512 }, AsInt = true, Order = 3)]
        protected IDiffSpread<Vector2> FInSize;

        [Input("Texture Count", DefaultValue = 1, Order = 4)]
        protected IDiffSpread<int> FInElementCount;

        [Input("Clear", DefaultValue = 1, Order = 6)]
        protected ISpread<bool> FInClear;

        [Input("Mips", DefaultValue = 0, Order = 5)]
        protected IDiffSpread<bool> FInMips;

        [Input("Clear Depth", DefaultValue = 1, Order = 6)]
        protected ISpread<bool> FInClearDepth;

        [Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 1 }, Order = 7)]
        protected ISpread<Color4> FInBgColor;

        [Input("Enabled", DefaultValue = 1, Order = 9)]
        protected ISpread<bool> FInEnabled;

        [Input("Enable Depth Buffer", Order = 9, DefaultValue = 1)]
        protected IDiffSpread<bool> FInDepthBuffer;

        [Input("View", Order = 11)]
        protected IDiffSpread<Matrix> FInView;

        [Input("Projection", Order = 12)]
        protected IDiffSpread<Matrix> FInProjection;

        [Input("Aspect Ratio", Order = 13, Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<Matrix> FInAspect;

        [Input("Crop", Order = 13, Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<Matrix> FInCrop;

        [Input("ViewPort", Order = 20)]
        protected Pin<Viewport> FInViewPort;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        [Output("Texture Out", Order = 2)]
        protected ISpread<DX11Resource<DX11RenderTarget2D>> FOutTexture;

        [Output("Depth Out", Order = 4)]
        protected ISpread<DX11Resource<DX11DepthStencil>> FOutDepthTexture;

        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        protected SampleDescription sd = new SampleDescription(1, 0);

        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();
        private int spmax;

        private DX11RenderSettings settings = new DX11RenderSettings();

        #region Constructor
        [ImportingConstructor()]
        public DX11TextureSpreadRendererNode(IPluginHost FHost, IIOFactory iofactory)
        {
            string ename = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.RenderTarget);

            InputAttribute tattr = new InputAttribute("Target Format");
            tattr.EnumName = ename;
            tattr.DefaultEnumEntry = "R8G8B8A8_UNorm";

            ConfigAttribute dfAttr = new ConfigAttribute("Depth Buffer Format");
            dfAttr.EnumName = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.DepthStencil);
            dfAttr.DefaultEnumEntry = "D32_Float";
            dfAttr.IsSingle = true;

            this.depthformatpin = iofactory.CreateDiffSpread<EnumEntry>(dfAttr);
            this.depthformatpin[0] = new EnumEntry(dfAttr.EnumName, 1);

            this.FInFormat = iofactory.CreateDiffSpread<EnumEntry>(tattr);
        }
        #endregion

        public void Evaluate(int SpreadMax)
        {
            this.spmax = SpreadMax;
            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            if (this.FInFormat.IsChanged
                || this.FInSize.IsChanged
                || this.FInElementCount.IsChanged
                || this.FInMips.IsChanged
                || this.FInDepthBuffer.IsChanged
                || this.depthformatpin.IsChanged)
            {


                for (int i = 0; i < this.FInElementCount[0]; i++)
                {
                    if (this.FOutTexture[i] != null)
                    {
                        this.FOutTexture[i].Dispose();
                        this.FOutTexture[i] = null;
                    }
                    if (this.FOutDepthTexture.SliceCount > 0)
                    {
                        if (this.FOutDepthTexture[i] != null)
                        {
                            this.FOutDepthTexture[i].Dispose();
                            this.FOutDepthTexture[i] = null;
                        }
                    }
                }

                this.FOutTexture.SliceCount = this.FInElementCount[0];
                if (this.FInDepthBuffer[0])
                {
                    this.FOutDepthTexture.SliceCount = this.FInElementCount[0];

                }
                else
                {
                    this.FOutDepthTexture.SliceCount = 0;
                }

                for (int i = 0; i < this.FInElementCount[0]; i++)
                {
                    if (this.FOutTexture[i] == null) { this.FOutTexture[i] = new DX11Resource<DX11RenderTarget2D>(); }

                    if (this.FOutDepthTexture.SliceCount > 0)
                    {
                        if (this.FOutDepthTexture[i] == null) { this.FOutDepthTexture[i] = new DX11Resource<DX11DepthStencil>(); }
                    }
                }
            }

        }

        public void Update(DX11RenderContext context)
        {
            Device device = context.Device;

            if (this.spmax == 0) { return; }

            if (this.updateddevices.Contains(context)) { return; }

            if (!this.FOutTexture[0].Contains(context))
            {
                for (int i = 0; i < this.FInElementCount[0]; i++)
                {
                    var rt = new DX11RenderTarget2D(context, (int)this.FInSize[i].X, (int)this.FInSize[i].Y, new SampleDescription(1, 0), DeviceFormatHelper.GetFormat(this.FInFormat[i]), this.FInMips[i], 0, false, false);
                    this.FOutTexture[i][context] = rt;
                    if (this.FInDepthBuffer[0])
                    {
                        Format depthfmt = DeviceFormatHelper.GetFormat(this.depthformatpin[0].Name);
                        var db = new DX11DepthStencil(context, (int)this.FInSize[i].X, (int)this.FInSize[i].Y, new SampleDescription(1, 0), depthfmt);
                        this.FOutDepthTexture[i][context] = db;
                    }
                }
            }
            this.updateddevices.Add(context);
        }

        public void Render(DX11RenderContext context)
        {
            Device device = context.Device;

            //Just in case
            if (!this.updateddevices.Contains(context))
            {
                this.Update(context);
            }

            if (this.rendereddevices.Contains(context)) { return; }

            if (this.FInEnabled[0])
            {
                if (this.BeginQuery != null)
                {
                    this.BeginQuery(context);
                }

                int sliceCount = this.FInElementCount[0];

                for (int i = 0; i < sliceCount; i++)
                {
                    DX11RenderTarget2D target = this.FOutTexture[i][context];

                    bool viewportpop = this.FInViewPort.IsConnected;

                    if (this.FInClear[i])
                    {
                        context.CurrentDeviceContext.ClearRenderTargetView(target.RTV, this.FInBgColor[i]);

                        if (this.FInDepthBuffer[0])
                        {
                            DX11DepthStencil depth = this.FOutDepthTexture[i][context];
                            context.CurrentDeviceContext.ClearDepthStencilView(depth.DSV, DepthStencilClearFlags.Depth, 1.0f, 0);
                            context.RenderTargetStack.Push(depth, false, target);
                        }
                        else
                        {
                            context.RenderTargetStack.Push(target);
                        }

                    }

                    if (this.FInLayer.IsConnected)
                    {
                        settings.ViewportIndex = i;
                        settings.ViewportCount = sliceCount;
                        settings.ApplyTransforms(this.FInView[i], this.FInProjection[i], this.FInAspect[i], this.FInCrop[i]);

                        settings.RenderWidth = target.Width;
                        settings.RenderHeight = target.Height;
                        settings.RenderDepth = 1;
                        settings.BackBuffer = null;
                        settings.CustomSemantics.Clear();
                        settings.ResourceSemantics.Clear();

                        if (viewportpop)
                        {
                            float cw = this.FInSize[i].X;
                            float ch = this.FInSize[i].Y;

                            context.RenderTargetStack.PushViewport(this.FInViewPort[i].Normalize(cw, ch));
                        }

                        for (int j = 0; j < this.FInLayer.SliceCount; j++)
                        {
                            try
                            {
                                this.FInLayer[j][context].Render(context, settings);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                        if (viewportpop)
                        {
                            context.RenderTargetStack.Pop();
                        }
                    }

                    context.RenderTargetStack.Pop();


                    if (this.FInMips[i])
                    {
                        context.CurrentDeviceContext.GenerateMips(this.FOutTexture[i][context].SRV);
                    }
                }

                if (this.EndQuery != null)
                {
                    this.EndQuery(context);
                }

                this.rendereddevices.Add(context);
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutTexture.SafeDisposeAll(context);
            this.FOutDepthTexture.SafeDisposeAll(context);
        }

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }

        public void Dispose()
        {
            this.FOutTexture.SafeDisposeAll();
            this.FOutDepthTexture.SafeDisposeAll();
        }
    }
}
