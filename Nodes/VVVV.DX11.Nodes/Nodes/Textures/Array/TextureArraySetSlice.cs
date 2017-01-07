using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using FeralTic.Resources.Geometry;
using SlimDX.Direct3D11;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using VVVV.Utils.VMath;

namespace VVVV.DX11.Nodes
{
    public class TextureArraySetSlice : IDX11Resource, IDisposable
    {
        private DX11RenderContext context;

        private DX11ShaderInstance shader;

        private DX11IndexedGeometry quad;

        private InputLayout layout;

        private DX11RenderTextureArray rtarr;
        public DX11RenderTextureArray Result { get { return rtarr; } }


        public TextureArraySetSlice(DX11RenderContext context)
        {
            this.context = context;
            this.shader = ShaderUtils.GetShader(context, "SetSlice");

            this.quad = context.Primitives.FullScreenQuad;
            this.quad.ValidateLayout(this.shader.GetPass(0), out this.layout);
        }

        public void Reset(DX11Texture2D texture, int w, int h, int d, SlimDX.DXGI.Format format)
        {
            format = format == SlimDX.DXGI.Format.Unknown ? texture.Format : format;
            this.rtarr.Dispose();
            this.rtarr = new DX11RenderTextureArray(this.context, w, h, d, format, true, 1);
        }

        public void Apply(DX11Texture2D texture, int w, int h, int d, SlimDX.DXGI.Format format, int slice)
        {
            format = format == SlimDX.DXGI.Format.Unknown ? texture.Format : format;

            if (this.rtarr != null)
            {
                if (this.rtarr.ElemCnt != d || this.rtarr.Width != w || this.rtarr.Height != h
                    || this.rtarr.Format != format)
                {
                    this.rtarr.Dispose(); this.rtarr = null;
                }
            }

            if (this.rtarr == null)
            {
                this.rtarr = new DX11RenderTextureArray(this.context, w, h, d, format, true, 1);
            }

            this.shader.SelectTechnique("Render");
            this.quad.Bind(this.layout);

            int idx = VMath.Zmod(slice, d);

            //Push specific slice as render target
            this.context.RenderTargetStack.Push(this.rtarr.SliceRTV[idx]);

            //Call simple shader (could use full screen triangle instead)
            this.shader.SetBySemantic("TEXTURE", texture.SRV);
            this.shader.ApplyPass(0);
            this.quad.Draw();
            this.context.RenderTargetStack.Pop();

        }

        public void Dispose()
        {
            //Do not dispose quad it's shared by everything
            if (this.shader != null) { this.shader.Dispose(); }
            if (this.rtarr != null) { this.rtarr.Dispose(); }
            if (this.layout != null) { this.layout.Dispose(); }          
        }
    }
}
