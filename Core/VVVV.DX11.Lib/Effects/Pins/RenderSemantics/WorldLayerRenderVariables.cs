using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeralTic.DX11;
using SlimDX;
using SlimDX.Direct3D11;
using VVVV.DX11.Lib.Effects.RenderSemantics;

namespace VVVV.DX11.Lib.Effects.Pins.RenderSemantics
{
    public class MatrixWorldLayerRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldLayerRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (r, obj) => effectVar.SetMatrix(obj.WorldTransform * r.WorldTransform);
        }
    }

    public class MatrixWorldLayerInverseTransposeRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldLayerInverseTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (r, obj) => effectVar.SetMatrix(Matrix.Transpose(Matrix.Invert(obj.WorldTransform * r.WorldTransform)));
        }
    }

    public class MatrixWorldLayerViewRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldLayerViewRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (r, obj) => effectVar.SetMatrix(obj.WorldTransform * r.WorldTransform * r.View);
        }
    }

    public class MatrixWorldLayerViewProjectionRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldLayerViewProjectionRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (r, obj) => effectVar.SetMatrix(obj.WorldTransform * r.WorldTransform * r.ViewProjection);
        }
    }
}
