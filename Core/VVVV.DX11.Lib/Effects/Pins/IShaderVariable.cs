using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public interface IShaderVariable : IDisposable
    {
        string Name { get; }
        string TypeName { get; }
        int Elements { get; }
        void Update(EffectVariable variable);
    }
}
