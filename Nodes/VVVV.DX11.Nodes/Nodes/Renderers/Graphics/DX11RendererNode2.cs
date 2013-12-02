using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11.Lib.Devices;
using FeralTic.DX11.Queries;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;
using VVVV.PluginInterfaces.V1;
using FeralTic.DX11.Resources;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using System.ComponentModel.Composition;

namespace VVVV.DX11.Nodes.Renderers.Graphics
{
    [PluginInfo(Name = "Renderer2", Category = "DX11", Author = "vux", AutoEvaluate = true,
        InitialWindowHeight = 300, InitialWindowWidth = 400, InitialBoxWidth = 400, InitialBoxHeight = 300, InitialComponentMode = TComponentMode.InAWindow)]
    public class DX11RendererNode2 : AbstractDX11Renderer2DNode, IDX11RenderWindow, IWin32Window, ICustomQueryInterface
    {
        private Control ctrl;

        private DX11RenderSettings settings = new DX11RenderSettings();

        [Output("Back Buffer")]
        protected ISpread<DX11Resource<DX11SwapChain>> FOutBackBuffer;

        [Input("VSync")]
        protected ISpread<bool> FInVsync;

        private bool resized = false;
        private bool invalidatesc;

        IDiffSpread<EnumEntry> FCfgBackBufferFormat;

        [ImportingConstructor()]
        public DX11RendererNode2(IPluginHost host, IIOFactory iofactory)
        {
            this.ctrl = new Control();
            this.ctrl.Dock = DockStyle.Fill;
            this.ctrl.Resize += new EventHandler(ctrl_Resize);

            this.depthmanager = new DepthBufferManager(host, iofactory);

            ConfigAttribute bbAttr = new ConfigAttribute("Back Buffer Format");
            bbAttr.IsSingle = true;
            bbAttr.EnumName = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.BackBufferCast);
            bbAttr.DefaultEnumEntry = DX11EnumFormatHelper.NullDeviceFormats.GetAllowedFormats(FormatSupport.BackBufferCast)[0];


            this.FCfgBackBufferFormat = iofactory.CreateDiffSpread<EnumEntry>(bbAttr);
            this.FCfgBackBufferFormat[0] = new EnumEntry(DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.BackBufferCast), 0);
            this.FCfgBackBufferFormat.Changed += new SpreadChangedEventHander<EnumEntry>(FCfgBackBufferFormat_Changed);
        }

        void ctrl_Resize(object sender, EventArgs e)
        {
            this.resized = true;
        }

        private void FCfgBackBufferFormat_Changed(IDiffSpread<EnumEntry> spread)
        {
            this.invalidatesc = true;
        }
        

        protected override void OnEvaluate(int SpreadMax)
        {
            if (this.FOutBackBuffer[0] == null) { this.FOutBackBuffer[0] = new DX11Resource<DX11SwapChain>(); }

            this.width = this.ctrl.Width;
            this.height = this.ctrl.Height;


        }

        protected override void OnUpdate(DX11RenderContext context)
        {
            var maxSamples = SlimDX.Direct3D11.Device.MultisampleCountMaximum;
            SampleDescription sd = new SampleDescription((int)Math.Min(FInAASamplesPerPixel[0], maxSamples), FInAAQuality[0]);

            if (this.resized || this.invalidatesc || this.FOutBackBuffer[0][context] == null)
            {
                EnumEntry bbf = this.FCfgBackBufferFormat[0];

                this.FOutBackBuffer[0].Dispose(context);

                //NOTE ENUM BROKEN
                Format fmt = (Format)Enum.Parse(typeof(Format), this.FCfgBackBufferFormat[0].Name);

                this.FOutBackBuffer[0][context] = new DX11SwapChain(context, this.Handle, fmt, sd);
                this.depthmanager.NeedReset = true;
            }
        }

        protected override void OnDestroy(DX11RenderContext context, bool force)
        {
            this.FOutBackBuffer[0].Dispose(context);
        }

        protected override void BeforeRender(DX11GraphicsRenderer renderer, DX11RenderContext context)
        {
            renderer.EnableDepth = this.FInDepthBuffer[0];
            renderer.DepthStencil = this.depthmanager.GetDepthStencil(context);
            renderer.DepthMode = this.depthmanager.Mode;
            renderer.SetRenderTargets(this.FOutBackBuffer[0][context]);
        }

        protected override void AfterRender(DX11GraphicsRenderer renderer, DX11RenderContext context)
        {

        }

        protected override void OnDispose()
        {
            if (this.FOutBackBuffer[0] != null)
            {
                this.FOutBackBuffer[0].Dispose();
            }
        }

        #region Random Stuff
        public IntPtr Handle
        {
            get { return ctrl.Handle; }
        }

        public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
        {
            if (iid.Equals(Guid.Parse("00000112-0000-0000-c000-000000000046")))
            {
                ppv = Marshal.GetComInterfaceForObject(ctrl, typeof(IOleObject));
                return CustomQueryInterfaceResult.Handled;
            }
            else if (iid.Equals(Guid.Parse("458AB8A2-A1EA-4d7b-8EBE-DEE5D3D9442C")))
            {
                ppv = Marshal.GetComInterfaceForObject(ctrl, typeof(IWin32Window));
                return CustomQueryInterfaceResult.Handled;
            }
            else
            {
                ppv = IntPtr.Zero;
                return CustomQueryInterfaceResult.NotHandled;
            }
        }
        #endregion

        #region Window Stuff
        public DX11RenderContext RenderContext
        {
            get;
            set;
        }

        public IntPtr WindowHandle
        {
            get { return ctrl.Handle; }
        }

        public bool IsVisible
        {
            get { return ctrl.Visible; }
        }

        public void Present()
        {
            this.resized = false;
            this.invalidatesc = false;
            if (ctrl.Visible)
            {
                try
                {
                    if (this.FInVsync[0])
                    {
                        this.FOutBackBuffer[0][this.RenderContext].Present(1, PresentFlags.None);
                    }
                    else
                    {
                        this.FOutBackBuffer[0][this.RenderContext].Present(0, PresentFlags.None);
                    }
                }
                catch
                {
                }
            }
        }
        #endregion
    }
}
