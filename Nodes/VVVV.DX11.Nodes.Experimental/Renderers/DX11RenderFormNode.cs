using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using FeralTic.DX11.Queries;
using VVVV.PluginInterfaces.V1;
using FeralTic.DX11;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11.Resources;
using System.Windows.Forms;
using SlimDX;
using VVVV.Utils.VColor;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using SlimDX.DXGI;

using Device = SlimDX.Direct3D11.Device;
using VVVV.DX11.Windows;

namespace VVVV.DX11.Nodes.Nodes.Renderers.Graphics
{
    [PluginInfo(Name="Renderer",Category="DX11", Version="Form", Author="vux",AutoEvaluate=true,
        InitialWindowHeight=300,InitialWindowWidth=400,InitialBoxWidth=400,InitialBoxHeight=300, InitialComponentMode=TComponentMode.InAWindow)]
    public class DX11RenderFormNode2 : IPluginEvaluate, IDisposable, IDX11RenderWindow,IDX11RendererHost
    {
        #region Input Pins
        IPluginHost FHost;

        private WindowDisplayCursor displayCursor;

        protected IHDEHost hde;
        [Import()]
        protected IPluginHost2 host2;

        [Import()]
        protected ILogger logger;



        [Input("Layers", Order = -10, IsSingle = true)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Position", AsInt = true)]
        protected IDiffSpread<Vector2> FInPosition;

        [Input("Size", AsInt = true, DefaultValues = new double[] { 400, 300 })]
        protected IDiffSpread<Vector2> FInSize;

        [Input("AA Samples per Pixel", DefaultEnumEntry = "1", EnumName = "DX11_AASamples")]
        protected IDiffSpread<EnumEntry> FInAASamplesPerPixel;

        [Input("Clear", DefaultValue = 1)]
        protected ISpread<bool> FInClear;

        [Input("Full Screen Resolution", DefaultValues = new double[] { 1920,1200 }, AsInt=true)]
        protected ISpread<Vector2> FInRes;

        [Input("Border",DefaultValue=1)]
        protected IDiffSpread<bool> FInBorder;

        [Input("TopMost")]
        protected IDiffSpread<bool> FInTopMost;

        [Input("Resize", IsBang=true)]
        protected ISpread<bool> FInResize;

        [Input("Rate", Visibility = PinVisibility.OnlyInspector,DefaultValue=30)]
        protected IDiffSpread<int> FInRate;

        [Input("Flip Sequential", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<bool> FInFlipSequential;

        [Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 1 }, Order = 3)]
        protected ISpread<RGBAColor> FInBgColor;

        [Input("VSync")]
        protected ISpread<int> FInVsync;

        [Input("Show Cursor", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<bool> FInShowCursor;

        [Input("Fullscreen", Order = 5)]
        protected IDiffSpread<bool> FInFullScreen;

        [Input("Enabled", DefaultValue = 1, Order = 9)]
        protected ISpread<bool> FInEnabled;


        [Output("Is Fullscreen", Order = 5)]
        protected ISpread<bool> FOutIsFullScreen;
        #endregion

        #region Fields
        private List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        private List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();

        private DX11RenderSettings settings = new DX11RenderSettings();

        private bool FInvalidateSwapChain;
        private bool FResized = false;
        private DX11SwapChain swapchain;
        private Form form;
        private DX11GraphicsRenderer renderer;
        private DepthBufferManager depthmanager;

        private int prevx = 400;
        private int prevy = 300;
        #endregion

		[ImportingConstructor()]
        public DX11RenderFormNode2(IPluginHost host, IIOFactory iofactory, IHDEHost hdehost)
        {
			this.FHost = host;
            this.hde = hdehost;

            this.form = new Form();
            this.form.Width = 400;
            this.form.Height = 300;
            //this.form.
            //this.form.ResizeEnd += form_ResizeEnd;
            this.form.Show();
            this.form.ShowIcon = false;
            this.displayCursor = new WindowDisplayCursor(this.form);

            this.depthmanager = new DepthBufferManager(host, iofactory);

        }

        void form_ResizeEnd(object sender, EventArgs e)
        {
            this.FInvalidateSwapChain = true;
        }

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FInShowCursor.IsChanged)
            {
                this.displayCursor.HideCursor = !this.FInShowCursor[0];
            }

            this.FInvalidateSwapChain = false;

            if (this.FInTopMost.IsChanged)
            {
                this.form.TopMost = this.FInTopMost[0];
            }

            if (this.FInBorder.IsChanged)
            {
                this.SetBorder();
            }


            if (!this.depthmanager.FormatChanged) // do not clear reset if format changed
            {
                this.depthmanager.NeedReset = false;
            }
            else
            {
                this.depthmanager.FormatChanged = false; //Clear flag ok
            }

            if (this.FInResize[0] || this.FInRate.IsChanged || this.FInFlipSequential.IsChanged || FInAASamplesPerPixel.IsChanged)
            {
                this.FInvalidateSwapChain = true;
                this.depthmanager.NeedReset = true;
            }

            if (this.FInPosition.IsChanged || this.FInSize.IsChanged)
            {
                this.form.Left = (int)this.FInPosition[0].X;
                this.form.Top = (int)this.FInPosition[0].Y;

                this.form.Width = (int)this.FInSize[0].X;
                this.form.Height = (int)this.FInSize[0].Y;

                this.FInvalidateSwapChain = true;
                this.depthmanager.NeedReset = true;
            }

            this.updateddevices.Clear();
            this.rendereddevices.Clear();
            
        }
        #endregion

        private void SetBorder()
        {
            this.form.FormBorderStyle = this.FInBorder[0] ? FormBorderStyle.Fixed3D : FormBorderStyle.None;
            this.FInvalidateSwapChain = true;
        }

        #region Update
        public void Update(DX11RenderContext context)
        {
            Device device = context.Device;

            if (this.updateddevices.Contains(context)) { return; }

            int samplecount = Convert.ToInt32(FInAASamplesPerPixel[0].Name);

            SampleDescription sd = new SampleDescription(samplecount, 0);

            if (this.FResized || this.FInvalidateSwapChain || this.swapchain == null)
            {
                List<SampleDescription> sds = context.GetMultisampleFormatInfo(Format.R8G8B8A8_UNorm);
                int maxlevels = sds[sds.Count - 1].Count;

                if (sd.Count > maxlevels)
                {
                    logger.Log(LogType.Warning, "Multisample count too high for this format, reverted to: " + maxlevels);
                    sd.Count = maxlevels;
                }


                if (this.swapchain != null) { this.swapchain.Dispose(); }
                this.swapchain = new DX11SwapChain(context, this.form.Handle, Format.R8G8B8A8_UNorm, sd,this.FInRate[0], 2, this.FInFlipSequential[0]);
                this.FInvalidateSwapChain = false;
            }

            if (this.renderer == null) { this.renderer = new DX11GraphicsRenderer(context); }

            bool fs = this.swapchain.IsFullScreen;

            if (this.FInFullScreen[0] != fs)
            {
                if (this.FInFullScreen[0])
                {
                    this.prevx = this.form.Width;
                    this.prevy = this.form.Height;

                    /*Screen screen = Screen.FromControl(this.form);*/
                    this.form.FormBorderStyle = FormBorderStyle.None;
                    this.form.Width = Convert.ToInt32(this.FInRes[0].X);
                    this.form.Height = Convert.ToInt32(this.FInRes[0].Y);
                    this.depthmanager.NeedReset = true;

                    this.swapchain.Resize();

                    this.swapchain.SetFullScreen(true);
                }
                else
                {
                    this.swapchain.SetFullScreen(false);
                    this.form.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.form.Width = this.prevx;
                    this.form.Height = this.prevy;
                    this.swapchain.Resize();

                }
            }

            this.depthmanager.Update(context, this.swapchain.Width, this.swapchain.Height, sd);

            this.updateddevices.Add(context);
            this.FOutIsFullScreen[0] = fs;
        }
        #endregion

        #region Destroy
        public void Destroy(DX11RenderContext context, bool force)
        {
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (this.swapchain != null)
            {
                try
                {
                    this.swapchain.Dispose();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            this.depthmanager.Dispose();
            this.form.Dispose();
        }
        #endregion

        #region Is Enabled
        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }
        #endregion

        #region Render Window

        private DX11RenderContext attachedContext;

        public void AttachContext(DX11RenderContext renderContext)
        {
            this.attachedContext = renderContext;
        }

        public DX11RenderContext RenderContext
        {
            get { return this.attachedContext; }
        }

        public IntPtr WindowHandle
        {
            get { return this.form.Handle; }
        }

        public bool Enabled
        {
            get { return this.form.Visible; }
        }

        public void Present()
        {
            try
            {
                //if (this.FInVsync[0])
                //{
                    this.swapchain.Present(this.FInVsync[0], PresentFlags.None);
                //}
                /*else
                {
                   this.swapchain.Present(0, PresentFlags.None);
                }*/
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        #endregion

        #region Render
        public void Render(DX11RenderContext context)
        {
            Device device = context.Device;

            if (!this.updateddevices.Contains(context)) { this.Update(context); }

            if (this.rendereddevices.Contains(context)) { return; }

            if (this.FInEnabled[0])
            {

                renderer.DepthStencil = this.depthmanager.GetDepthStencil(context);
                renderer.EnableDepth = renderer.DepthStencil != null;
                renderer.DepthMode = this.depthmanager.Mode;
                renderer.SetRenderTargets(this.swapchain);
                renderer.SetTargets();

                if (this.FInClear[0])
                {
                    //Remove Shader view if bound as is
                    context.CurrentDeviceContext.ClearRenderTargetView(this.swapchain.RTV, this.FInBgColor[0].Color);
                    this.depthmanager.Clear(context);
                }

                //Only call render if layer connected
                if (this.FInLayer.IsConnected)
                {
                    float cw = (float)this.form.ClientSize.Width;
                    float ch = (float)this.form.ClientSize.Height;

                    settings.ViewportCount = 1;
                    settings.ViewportIndex = 0;
                    settings.View = Matrix.Identity;
                    settings.Projection = Matrix.Identity;
                    settings.ViewProjection = Matrix.Identity;
                    settings.BackBuffer = this.swapchain;
                    settings.RenderWidth = this.swapchain.Width;
                    settings.RenderHeight = this.swapchain.Height;
                    settings.ResourceSemantics.Clear();
                    settings.CustomSemantics.Clear();


                    this.FInLayer.RenderAll(context, settings);

                }
                renderer.CleanTargets();
            }
            this.rendereddevices.Add(context);
        }
        #endregion







    }
}
