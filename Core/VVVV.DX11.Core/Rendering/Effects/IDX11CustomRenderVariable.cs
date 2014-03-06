using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11.Effects
{
    public interface IDX11CustomRenderVariable
    {
        string Name { get; }
        string TypeName { get; }
        string Semantic { get; }
    }
}
