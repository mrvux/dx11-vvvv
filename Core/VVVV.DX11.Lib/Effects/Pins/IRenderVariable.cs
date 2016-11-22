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
        Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader);
    }

    public interface IWorldRenderVariable : IShaderVariable
    {
        string Semantic { get; }
        Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader);
    }
}
