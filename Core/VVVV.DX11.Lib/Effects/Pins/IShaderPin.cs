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
        string PinName { get; }
        bool Constant { get; }
        int SliceCount { get; }
        Action<int> CreateAction(DX11ShaderInstance instance);
    }

    public interface IMultiTypeShaderPin
    {
        bool ChangeType(EffectVariable var);
    }

    public interface IUpdateShaderPin
    {
        void Update();
    }


}
