using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;
using VVVV.DX11.Lib.Effects.RenderSemantics;
using VVVV.DX11.Internals;
using SlimDX.Direct3D11;
using SlimDX;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;

namespace VVVV.DX11.Lib.Effects.Pins.RenderSemantics
{
    public class MatrixLayerWorldRenderVariable : AbstractRenderVariable
    {
        public MatrixLayerWorldRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(s.WorldTransform);
        }
    }

    public class MatrixLayerInvWorldRenderVariable : AbstractRenderVariable
    {
        public MatrixLayerInvWorldRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(Matrix.Invert(s.WorldTransform));
        }
    }

    public class MatrixProjRenderVariable : AbstractRenderVariable
    {
        public MatrixProjRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(s.Projection);
        }
    }

    public class MatrixInvProjRenderVariable : AbstractRenderVariable
    {
        public MatrixInvProjRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(Matrix.Invert(s.Projection));
        }
    }

    public class MatrixProjTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixProjTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(Matrix.Transpose(s.Projection));
        }
    }

    public class MatrixInvProjTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixInvProjTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(Matrix.Transpose(Matrix.Invert(s.Projection)));
        }
    }

    public class MatrixLayerWorldViewRenderVariable : AbstractRenderVariable
    {
        public MatrixLayerWorldViewRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(s.WorldTransform * s.View);
        }
    }

    public class MatrixViewRenderVariable : AbstractRenderVariable
    {
        public MatrixViewRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(s.View);
        }
    }

    public class MatrixInvViewRenderVariable : AbstractRenderVariable
    {
        public MatrixInvViewRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(Matrix.Invert(s.View));
        }
    }

    public class CameraPositionRenderVariable : AbstractRenderVariable
    {
        public CameraPositionRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsVector();
            return (s) => { Matrix iv = Matrix.Invert(s.View); sv.Set(new Vector3(iv.M41, iv.M42, iv.M43)); };
        }
    }

    public class MatrixInvViewTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixInvViewTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(Matrix.Transpose(Matrix.Invert(s.View)));
        }
    }

    public class MatrixViewProjRenderVariable : AbstractRenderVariable
    {
        public MatrixViewProjRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(s.ViewProjection);
        }
    }

    public class MatrixLayerWorldViewProjRenderVariable : AbstractRenderVariable
    {
        public MatrixLayerWorldViewProjRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(s.WorldTransform * s.ViewProjection);
        }
    }


    public class MatrixInvViewProjRenderVariable : AbstractRenderVariable
    {
        public MatrixInvViewProjRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(Matrix.Invert(s.ViewProjection));
        }
    }

    public class MatrixInvViewProjTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixInvViewProjTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(Matrix.Transpose(Matrix.Invert(s.ViewProjection)));
        }
    }


    public class MatrixViewProjTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixViewProjTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(Matrix.Transpose(s.ViewProjection));
        }
    }

    public class MatrixViewTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixViewTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (s) => sv.SetMatrix(Matrix.Transpose(s.View));
        }
    }
}
