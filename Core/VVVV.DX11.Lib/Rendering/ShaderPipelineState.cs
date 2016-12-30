using FeralTic.DX11;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.Lib.Rendering
{
    public class ShaderPipelineState
    {
        private VertexShader VertexShader;
        private HullShader HullShader;
        private DomainShader DomainShader;
        private GeometryShader GeometryShader;
        private PixelShader PixelShader;

        public ShaderPipelineState(DX11RenderContext renderContext)
        {
            var context = renderContext.CurrentDeviceContext;
            this.VertexShader = context.VertexShader.Get();
            this.HullShader = context.HullShader.Get();
            this.DomainShader = context.DomainShader.Get();
            this.GeometryShader = context.GeometryShader.Get();
            this.PixelShader = context.PixelShader.Get();
        }

        public void Restore(DX11RenderContext renderContext)
        {
            var context = renderContext.CurrentDeviceContext;
            context.VertexShader.Set(VertexShader);
            context.HullShader.Set(HullShader);
            context.DomainShader.Set(DomainShader);
            context.GeometryShader.Set(GeometryShader);
            context.PixelShader.Set(PixelShader);
        }
    }
}
