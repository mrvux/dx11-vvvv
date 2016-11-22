using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Lib.Effects.RenderSemantics;
using VVVV.DX11.Internals;
using SlimDX.Direct3D11;
using SlimDX;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;

namespace VVVV.DX11.Lib.Effects.Pins.RenderSemantics
{
    public class MatrixWorldRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (r, o) => effectVar.SetMatrix(o.WorldTransform);
        }
    }

    public class MatrixWorldInvRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldInvRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (r, obj) => effectVar.SetMatrix(Matrix.Invert(obj.WorldTransform));
        }
    }

    public class MatrixWorldTransposeRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (r, obj) => effectVar.SetMatrix(Matrix.Transpose(obj.WorldTransform));
        }
    }

    public class MatrixWorldInverseTransposeRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldInverseTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (r, obj) => effectVar.SetMatrix(Matrix.Transpose(Matrix.Invert(obj.WorldTransform)));
        }
    }


    public class MatrixWorldViewRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldViewRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (settings, obj) => effectVar.SetMatrix(obj.WorldTransform * settings.View);
        }
    }

    public class MatrixWorldViewProjRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldViewProjRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (settings, obj) => effectVar.SetMatrix(obj.WorldTransform * settings.ViewProjection);
        }
    }

    public class IntDrawIndexRenderVariable : AbstractWorldRenderVariable
    {
        public IntDrawIndexRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (settings, obj) => effectVar.Set(obj.DrawCallIndex);
        }
    }

    public class FloatDrawIndexRenderVariable : AbstractWorldRenderVariable
    {
        public FloatDrawIndexRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (settings, obj) => effectVar.Set((float)obj.DrawCallIndex);
        }
    }

    public class IterCountRenderVariable : AbstractWorldRenderVariable
    {
        public IterCountRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (settings, obj) => effectVar.Set(obj.IterationCount);
        }
    }

    public class IterIndexRenderVariable : AbstractWorldRenderVariable
    {
        public IterIndexRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (settings, obj) => effectVar.Set(obj.IterationIndex);
        }
    }
}
