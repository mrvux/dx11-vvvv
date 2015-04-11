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

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj)
        {
            shaderinstance.SetByName(this.Name, obj.WorldTransform);
        }
    }

    public class MatrixWorldInvRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldInvRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj)
        {
            shaderinstance.SetByName(this.Name, Matrix.Invert(obj.WorldTransform));
        }
    }

    public class MatrixWorldTransposeRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj)
        {
            shaderinstance.SetByName(this.Name, Matrix.Transpose(obj.WorldTransform));
        }
    }

    public class MatrixWorldInverseTransposeRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldInverseTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj)
        {
            shaderinstance.SetByName(this.Name, Matrix.Transpose(Matrix.Invert(obj.WorldTransform)));
        }
    }


    public class MatrixWorldViewRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldViewRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj)
        {
            shaderinstance.SetByName(this.Name, obj.WorldTransform * settings.View);
        }
    }

    public class MatrixWorldViewProjRenderVariable : AbstractWorldRenderVariable
    {
        public MatrixWorldViewProjRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj)
        {
            shaderinstance.SetByName(this.Name, obj.WorldTransform * settings.ViewProjection);
        }
    }

    public class IntDrawIndexRenderVariable : AbstractWorldRenderVariable
    {
        public IntDrawIndexRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj)
        {
            shaderinstance.SetByName(this.Name, obj.DrawCallIndex);
        }
    }

    public class FloatDrawIndexRenderVariable : AbstractWorldRenderVariable
    {
        public FloatDrawIndexRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj)
        {
            shaderinstance.SetByName(this.Name, (float)obj.DrawCallIndex);
        }
    }

    public class IterCountRenderVariable : AbstractWorldRenderVariable
    {
        public IterCountRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj)
        {
            shaderinstance.SetByName(this.Name, obj.IterationCount);
        }
    }

    public class IterIndexRenderVariable : AbstractWorldRenderVariable
    {
        public IterIndexRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj)
        {
            shaderinstance.SetByName(this.Name, obj.IterationIndex);
        }
    }
}
