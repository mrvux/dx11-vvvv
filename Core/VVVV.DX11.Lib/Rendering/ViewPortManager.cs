using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using VVVV.DX11.Lib.Devices;
using System.Drawing;

using FeralTic.DX11;

namespace VVVV.DX11.Lib.Rendering
{
    public class ViewPortManager
    {
        private DX11RenderContext ctx;

        public ViewPortManager(DX11RenderContext context)
        {
            this.ctx = context;    
        }

        public void SetDefaultViewPort(float cw, float ch)
        {
            Viewport vp = new Viewport(0, 0, cw, ch);
            ctx.CurrentDeviceContext.Rasterizer.SetViewports(vp);
        }

        public void SetViewPort(float cw, float ch, Viewport nvp)
        {
            Viewport vp = new Viewport();
            vp.Width = nvp.Width * cw;
            vp.Height = nvp.Height * ch;

            float x = nvp.X / 2.0f + 0.5f;
            float y = 1.0f - (nvp.Y / 2.0f + 0.5f);
            vp.X = (x * cw) - (vp.Width / 2.0f);
            vp.Y = (y * ch) - (vp.Height / 2.0f);

            ctx.CurrentDeviceContext.Rasterizer.SetViewports(vp);
        }
    }
}
