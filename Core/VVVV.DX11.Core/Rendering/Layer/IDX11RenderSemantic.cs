using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FeralTic.DX11.Resources;
using FeralTic.DX11;

using VVVV.DX11.Effects;

namespace VVVV.DX11
{
    public interface IDX11RenderSemantic : IDX11Resource
    {
        string[] TypeNames { get; }
        string Semantic { get; }
        bool Mandatory { get; }
        bool Apply(DX11ShaderInstance instance, List<IDX11CustomRenderVariable> variables);
    }
}
