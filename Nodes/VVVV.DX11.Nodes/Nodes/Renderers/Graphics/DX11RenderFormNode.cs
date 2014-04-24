﻿using System;
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

namespace VVVV.DX11.Nodes.Nodes.Renderers.Graphics
{
    [PluginInfo(Name="RenderForm",Category="DX11",Author="vux,tonfilm",AutoEvaluate=true,
        InitialWindowHeight=300,InitialWindowWidth=400,InitialBoxWidth=400,InitialBoxHeight=300, InitialComponentMode=TComponentMode.InAWindow)]
    public class DX11RenderFormNode : IPluginEvaluate, IDisposable, IDX11RenderWindow,IDX11RendererProvider
    {
        #region Input Pins
        IPluginHost FHost;

        protected IHDEHost hde;
        [Import()]
        protected IPluginHost2 host2;

        [Import()]
        protected ILogger logger;

        [Input("TopMost")]
        protected IDiffSpread<bool> FInTopMost;

        [Input("Layers", Order = 1, IsSingle = true)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;



        [Input("Clear", DefaultValue = 1, Order = 2)]
        protected ISpread<bool> FInClear;

        [Input("Position", DefaultValues = new double[] { 50, 50 }, AsInt = true)]
        protected IDiffSpread<Vector2> FInPos;

        [Input("Res", DefaultValues = new double[] { 1920,1200 }, AsInt=true)]
        protected ISpread<Vector2> FInRes;

        [Input("Rate", Visibility = PinVisibility.OnlyInspector,DefaultValue=30)]
        protected ISpread<int> FInRate;

        [Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 1 }, Order = 3)]
        protected ISpread<RGBAColor> FInBgColor;

        [Input("VSync", Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FInVsync;

        [Input("Show Cursor", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<bool> FInShowCursor;

        [Input("Fullscreen", Order = 5)]
        protected IDiffSpread<bool> FInFullScreen;

        [Input("Enabled", DefaultValue = 1, Order = 9)]
        protected ISpread<bool> FInEnabled;
        #endregion

        #region Fields
        private List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        private List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();

        private DX11RenderSettings settings = new DX11RenderSettings();

        private bool FInvalidateSwapChain;
        private bool FResized = false;
        private DX11RenderContext primary;
        private DX11SwapChain swapchain;
        private Form form;
        private DX11GraphicsRenderer renderer;

        private int prevx = 400;
        private int prevy = 300;

        private bool setfull = false;
        #endregion

		[ImportingConstructor()]
        public DX11RenderFormNode(IPluginHost host, IIOFactory iofactory, IHDEHost hdehost)
        {
			this.FHost = host;
            this.hde = hdehost;

            this.form = new Form();
            this.form.Width = 400;
            this.form.Height = 300;
            this.form.Show();


            /*this.form.Resize += DX11RendererNode_Resize;
            this.form.Load += new EventHandler(DX11RendererNode_Load);*/

        
        }

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FInPos.IsChanged)
            {
                this.form.Top = Convert.ToInt32(this.FInPos[0].Y);
                this.form.Left = Convert.ToInt32(this.FInPos[0].X);
            }

            if (this.FInTopMost.IsChanged)
            {
                this.form.TopMost = this.FInTopMost[0];
            }

            this.updateddevices.Clear();
            this.rendereddevices.Clear();
            this.FInvalidateSwapChain = false;

            if (this.FInFullScreen.IsChanged)
            {
                this.setfull = true;
            }
        }
        #endregion

        #region Update
        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            Device device = context.Device;

            if (this.updateddevices.Contains(context)) { return; }

            SampleDescription sd = new SampleDescription(1, 0);

            if (this.FResized || this.FInvalidateSwapChain || this.swapchain == null)
            {
                if (this.swapchain != null) { this.swapchain.Dispose(); }
                this.swapchain = new DX11SwapChain(context, this.form.Handle, Format.R8G8B8A8_UNorm, sd,this.FInRate[0]);
            }

            if (this.renderer == null) { this.renderer = new DX11GraphicsRenderer(this.FHost, context); }
            this.updateddevices.Add(context);

            if (this.FInFullScreen[0] != this.swapchain.IsFullScreen)
            {
                if (this.FInFullScreen[0])
                {
                    this.prevx = this.form.Width;
                    this.prevy = this.form.Height;

                    /*Screen screen = Screen.FromControl(this.form);*/
                    this.form.FormBorderStyle = FormBorderStyle.None;
                    this.form.Width = Convert.ToInt32(this.FInRes[0].X);
                    this.form.Height = Convert.ToInt32(this.FInRes[0].Y);

                    this.swapchain.Resize();

                    this.swapchain.SetFullScreen(true);

                    this.setfull = false;
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
        }
        #endregion

        #region Destroy
        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
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
        }
        #endregion

        #region Is Enabled
        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }
        #endregion

        #region Render Window
        public DX11RenderContext RenderContext
        {
            get { return this.primary; }
            set
            {
                this.primary = value;
            }
        }

        public IntPtr WindowHandle
        {
            get { return this.form.Handle; }
        }

        public bool IsVisible
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

            if (!this.updateddevices.Contains(context)) { this.Update(null, context); }

            if (this.rendereddevices.Contains(context)) { return; }

            if (this.FInEnabled[0])
            {
                renderer.EnableDepth = false;
                renderer.DepthStencil = null;
                renderer.DepthMode = eDepthBufferMode.None;
                renderer.SetRenderTargets(this.swapchain);
                renderer.SetTargets();

                if (this.FInClear[0])
                {
                    //Remove Shader view if bound as is
                    context.CurrentDeviceContext.ClearRenderTargetView(this.swapchain.RTV, this.FInBgColor[0].Color);
                }

                //Only call render if layer connected
                if (this.FInLayer.PluginIO.IsConnected)
                {
                    float cw = (float)this.form.ClientSize.Width;
                    float ch = (float)this.form.ClientSize.Height;

                    settings.ViewportCount = 1;
                    settings.ViewportIndex = 0;
                    settings.View = Matrix.Identity;
                    settings.Projection = Matrix.Identity;
                    settings.ViewProjection = Matrix.Identity;
                    settings.BackBuffer = this.swapchain;
                    settings.RenderWidth = 1920;
                    settings.RenderHeight = 1200;
                    settings.ResourceSemantics.Clear();
                    settings.CustomSemantics.Clear();

                    //Call render on all layers
                    for (int j = 0; j < this.FInLayer.SliceCount; j++)
                    {
                        this.FInLayer[j][context].Render(this.FInLayer.PluginIO, context, settings);
                    }
                }
                renderer.CleanTargets();
            }
            this.rendereddevices.Add(context);
        }
        #endregion







    }
}
