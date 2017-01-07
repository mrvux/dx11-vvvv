using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using SlimDX;
using VVVV.DX11.Lib.Effects.Pins;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public class Float3ShaderPin : AbstractValuePin<Vector3>
    {
        protected override void SetDefault(InputAttribute attr, EffectVariable var)
        {
            Vector4 vec = var.AsVector().GetVector();
            attr.DefaultValues = new double[] { vec.X, vec.Y, vec.Z };
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsVector();
            return (i) => { sv.Set(this.pin[i]); };
        }
    }


}
