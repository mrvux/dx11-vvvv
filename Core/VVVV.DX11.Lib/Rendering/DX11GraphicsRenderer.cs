using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D11;
using VVVV.DX11.Lib.Devices;
using VVVV.PluginInterfaces.V1;
using VVVV.DX11.Lib;
using SlimDX;
using FeralTic.Resources;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Lib.Rendering
{
    public class DX11GraphicsRenderer
    {
        private DX11RenderContext context;
        private IDX11RenderTargetView[] rtvs;

        public ViewPortManager ViewPortManager { get; protected set; }

        public void SetRenderTargets(params IDX11RenderTargetView[] irtvs)
        {
            this.rtvs = irtvs;
        }

        public IDX11DepthStencil DepthStencil { get; set; }

        public eDepthBufferMode DepthMode { get; set; }

        public bool EnableDepth { get; set; }

        public DX11GraphicsRenderer(DX11RenderContext context)
        {
            this.ViewPortManager = new ViewPortManager(context);
            this.context = context;
        }

        #region Set render targets
        public void SetTargets()
        {
            if (this.DepthMode != eDepthBufferMode.None && this.EnableDepth)
            {
                this.context.RenderTargetStack.Push(this.DepthStencil, this.DepthMode == eDepthBufferMode.ReadOnly, this.rtvs);
            }
            else
            {
                this.context.RenderTargetStack.Push(this.rtvs);
            }

            /*if (this.DSV != null && this.EnableDepth)
            {
                this.context.CurrentDeviceContext.OutputMerger.SetTargets(this.DSV, this.rtvs);
            }
            else
            {
                this.context.CurrentDeviceContext.OutputMerger.SetTargets(this.rtvs);
            }*/
        }
        #endregion

        public void Clear(Color4 clearcolor)
        {
            foreach (IDX11RenderTargetView view in this.rtvs)
            {  
                this.context.CurrentDeviceContext.ClearRenderTargetView(view.RTV, clearcolor);
            }
        }

        public void CleanTargets()
        {
            this.context.RenderTargetStack.Pop();
        }


    }
}
