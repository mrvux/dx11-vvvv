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
    public class MatrixProjRenderVariable : AbstractRenderVariable
    {
        public MatrixProjRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, settings.Projection);
        }
    }

    public class MatrixInvProjRenderVariable : AbstractRenderVariable
    {
        public MatrixInvProjRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, Matrix.Invert(settings.Projection));
        }
    }

    public class MatrixProjTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixProjTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, Matrix.Transpose(settings.Projection));
        }
    }

    public class MatrixInvProjTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixInvProjTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, Matrix.Transpose(Matrix.Invert(settings.Projection)));
        }
    }

    public class MatrixViewRenderVariable : AbstractRenderVariable
    {
        public MatrixViewRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, settings.View);
        }
    }

    public class MatrixInvViewRenderVariable : AbstractRenderVariable
    {
        public MatrixInvViewRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, Matrix.Invert(settings.View));
        }
    }

    public class MatrixInvViewTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixInvViewTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, Matrix.Transpose(Matrix.Invert(settings.View)));
        }
    }

    public class MatrixViewProjRenderVariable : AbstractRenderVariable
    {
        public MatrixViewProjRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, settings.ViewProjection);
        }
    }


    public class MatrixInvViewProjRenderVariable : AbstractRenderVariable
    {
        public MatrixInvViewProjRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, Matrix.Invert(settings.ViewProjection));
        }
    }

    public class MatrixInvViewProjTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixInvViewProjTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, Matrix.Transpose(Matrix.Invert(settings.ViewProjection)));
        }
    }


    public class MatrixViewProjTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixViewProjTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, Matrix.Transpose(settings.ViewProjection));
        }
    }

    public class MatrixViewTransposeRenderVariable : AbstractRenderVariable
    {
        public MatrixViewTransposeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, Matrix.Transpose(settings.View));
        }
    }
}
