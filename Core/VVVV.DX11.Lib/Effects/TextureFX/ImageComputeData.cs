using FeralTic.DX11;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.Lib.Effects
{
    public class ImagePassComputeInfo
    {
        public ImagePassComputeInfo(EffectPass pass)
        {
            this.Enabled = pass.ComputeShaderDescription.Variable.IsValid;
            this.tX = pass.GetIntAnnotation("tx", 1);
            this.tY = pass.GetIntAnnotation("ty", 1);
            this.tZ = pass.GetIntAnnotation("tz", 1);
        }

        public bool Enabled { get; protected set; }
        public int tX { get; protected set; }
        public int tY { get; protected set; }
        public int tZ { get; protected set; }

        public void Dispatch(DX11RenderContext context, int w, int h)
        {
            context.CurrentDeviceContext.PixelShader.Set(null);

            int tgx = (w + (this.tX - 1)) / this.tX;
            int tgy = (h + (this.tY - 1)) / this.tY;

            context.CurrentDeviceContext.Dispatch(tgx, tgy, 1);
        }
    }
}
