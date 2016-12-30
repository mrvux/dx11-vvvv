using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FeralTic.DX11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using FeralTic.DX11.Resources;
using System.ComponentModel.Composition;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "WindowTester", Category = "DX11", Author = "vux", AutoEvaluate = true, InitialComponentMode = TComponentMode.InAWindow)]
    public class DX11RenderFullScreenTestForm : Control, IPluginEvaluate, IDX11RendererProvider, IDX11RenderWindow
    {
        [Import()]
        private INode node;

        [Input("Full Screen", Order = 5, IsBang = true)]
        protected IDiffSpread<bool> fullscreen;

        [Output("Is Full Screen")]
        protected ISpread<bool> isfullscreen;

        [Output("Handle")]
        protected ISpread<string> handle;

        private DX11SwapChain swapChain;

        public bool IsEnabled
        {
            get { return true; }
        }

        public bool IsVisible
        {
            get
            {
                return true;
            }
        }

        public DX11RenderContext RenderContext
        {
            get; set;
        }

        public IntPtr WindowHandle
        {
            get
            {
                return this.node.Window.Handle;
            }
        }

        private void CreateSwapChain()
        {
            if (this.swapChain == null && this.RenderContext != null)
            {
                this.swapChain = new DX11SwapChain(this.RenderContext, this.node.Window.Handle, SlimDX.DXGI.Format.R8G8B8A8_UNorm, new SlimDX.DXGI.SampleDescription(1, 0), 60, 2, false);
                this.RenderContext.Factory.SetWindowAssociation(this.node.Window.Handle, SlimDX.DXGI.WindowAssociationFlags.IgnoreAll);
            }
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            
        }

        public void Evaluate(int SpreadMax)
        {
            this.Visible = false;
            if (this.fullscreen[0])
            {
                this.CreateSwapChain();

                this.swapChain.Resize();

                this.swapChain.SetFullScreen(true);

                this.swapChain.Resize();
            }

            if (this.swapChain != null)
            {
                this.isfullscreen[0] = this.swapChain.IsFullScreen;
            }

            this.handle[0] = this.node.Window.Handle.ToString();
        }

        public void Present()
        {
            if (this.swapChain != null)
            {
                this.swapChain.Present(0, SlimDX.DXGI.PresentFlags.None);
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            this.CreateSwapChain();
        }

        public void Render(DX11RenderContext context)
        {
            this.CreateSwapChain();

            context.CurrentDeviceContext.ClearRenderTargetView(this.swapChain.RTV, new SlimDX.Color4(1, 0, 1, 0));
        }
    }
}
