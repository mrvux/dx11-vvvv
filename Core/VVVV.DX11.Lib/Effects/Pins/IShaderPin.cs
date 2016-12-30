using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX;
using VVVV.DX11.Lib.Effects;
using VVVV.PluginInterfaces.V2;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public interface IShaderPin : IShaderVariable
    {
        void Initialize(IIOFactory factory, EffectVariable variable);

        void SetVariable(DX11ShaderInstance shaderinstance, int slice);

        string PinName { get; }
        bool Constant { get; }
        int SliceCount { get; }
    }

    public interface IMultiTypeShaderPin
    {
        bool ChangeType(EffectVariable var);
    }


}
