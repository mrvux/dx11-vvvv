using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D11;
using VVVV.Hosting.IO;
using VVVV.DX11.Lib.Effects.Pins;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public abstract class AbstractArrayPin<T> : AbstractShaderV2Spread<ISpread<T>>
    {
        protected T[] array;

        protected override void ProcessAttribute(InputAttribute attr, EffectVariable var)
        {
            array = new T[var.GetVariableType().Description.Elements];
            attr.BinSize = array.Length;
        }

        protected abstract void UpdateShaderValue(DX11ShaderInstance shaderinstance);

        protected void UpdateArray(int slice)
        {
            ISpread<T> f = this.pin[slice];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = f[i];
            }
        }
 
        protected override bool RecreatePin(EffectVariable variable)
        {
            return false;
        }
    }
}
