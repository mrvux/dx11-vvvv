using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V2;

using VVVV.DX11.Lib.Effects.Pins;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public class FloatShaderPin : AbstractValuePin<float>
    {
        public override void SetVariable(DX11ShaderInstance shaderinstance, int slice)
        {
            shaderinstance.SetByName(this.Name, this.pin[slice]);
        }

        protected override void SetDefault(InputAttribute attr, EffectVariable var)
        {
            attr.DefaultValue = var.AsScalar().GetFloat();
        }
    }
}
