using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V2;

using VVVV.DX11.Lib.Effects.Pins;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public class BoolShaderPin : AbstractValuePin<bool>
    {
        private bool isbang;

        protected override void SetDefault(InputAttribute attr, EffectVariable var)
        {
            attr.DefaultValue = var.AsScalar().GetFloat();
            attr.IsBang = var.IsBang();
            this.isbang = attr.IsBang;
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsScalar();
            return (i) => { sv.Set(this.pin[i]); };
        }

        protected override bool RecreatePin(EffectVariable var)
        {
            return base.RecreatePin(var) || this.isbang != var.IsBang();
        }

    }
}
