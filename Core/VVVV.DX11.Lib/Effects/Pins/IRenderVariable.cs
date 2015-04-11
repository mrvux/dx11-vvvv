using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.DX11.Internals;
using VVVV.DX11.Lib.Effects.Pins;
using VVVV.DX11.Lib.Effects;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public interface IRenderVariable : IShaderVariable
    {
        string Semantic { get; }
        void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings);
    }

    public interface IWorldRenderVariable : IShaderVariable
    {
        string Semantic { get; }
        void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj);
    }
}
