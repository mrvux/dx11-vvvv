using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;

using VVVV.DX11.Lib.Rendering;

namespace VVVV.DX11.Nodes
{
    public abstract class DX11CustomSemanticNode<D, T> : IPluginEvaluate where T : IDX11RenderSemantic
    {
        [Input("Input")]
        protected ISpread<D> FInput;

        [Input("Semantic", DefaultString="SEMANTIC")]
        protected ISpread<string> FSemantic;

        [Input("Mandatory",DefaultValue =0)]
        protected ISpread<bool> FMandatory;

        [Output("Output")]
        protected ISpread<T> FOutput;

        protected abstract T GetData(D input, string semantic, bool mandatory);

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = this.FInput.SliceCount;

            for (int i = 0; i < SpreadMax; i++) { this.FOutput[i] = this.GetData(this.FInput[i], this.FSemantic[i], this.FMandatory[i]); }
        }
    }


    [PluginInfo(Name = "RenderSemantic", Category = "DX11.Layer", Version = "Value")]
    public class FloatSemanticNode : DX11CustomSemanticNode<float, FloatRenderSemantic>
    {
        protected override FloatRenderSemantic GetData(float input, string semantic, bool mandatory)
        {
            return new FloatRenderSemantic(semantic, mandatory, input);
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11.Layer", Version = "Int")]
    public class IntSemanticNode : DX11CustomSemanticNode<int, IntRenderSemantic>
    {
        protected override IntRenderSemantic GetData(int input, string semantic, bool mandatory)
        {
            return new IntRenderSemantic(semantic, mandatory, input);
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11.Layer", Version = "2d")]
    public class Vector2SemanticNode : DX11CustomSemanticNode<Vector2, Vector2RenderSemantic>
    {
        protected override Vector2RenderSemantic GetData(Vector2 input, string semantic, bool mandatory)
        {
            return new Vector2RenderSemantic(semantic, mandatory, input);
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11.Layer", Version = "3d")]
    public class Vector3SemanticNode : DX11CustomSemanticNode<Vector3, Vector3RenderSemantic>
    {
        protected override Vector3RenderSemantic GetData(Vector3 input, string semantic, bool mandatory)
        {
            return new Vector3RenderSemantic(semantic, mandatory, input);
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11.Layer", Version = "4d")]
    public class Vector4SemanticNode : DX11CustomSemanticNode<Vector4, Vector4RenderSemantic>
    {
        protected override Vector4RenderSemantic GetData(Vector4 input, string semantic, bool mandatory)
        {
            return new Vector4RenderSemantic(semantic, mandatory, input);
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11.Layer", Version = "Color")]
    public class Color4SemanticNode : DX11CustomSemanticNode<Color4, Vector4RenderSemantic>
    {
        protected override Vector4RenderSemantic GetData(Color4 input, string semantic, bool mandatory)
        {
            return new Vector4RenderSemantic(semantic, mandatory, input.ToVector4());
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11.Layer", Version = "Transform")]
    public class MatrixSemanticNode : DX11CustomSemanticNode<Matrix, MatrixRenderSemantic>
    {
        protected override MatrixRenderSemantic GetData(Matrix input, string semantic, bool mandatory)
        {
            return new MatrixRenderSemantic(semantic, mandatory, input);
        }
    }
}
