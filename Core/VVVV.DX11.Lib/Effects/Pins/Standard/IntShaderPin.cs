using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.Pins.Input;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.DX11.Lib.Effects.Pins;
using SlimDX;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public class IntShaderPin : AbstractValuePin<int>
    {
        protected override void SetDefault(InputAttribute attr, EffectVariable var)
        {
            attr.DefaultValue = var.AsScalar().GetFloat();
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsScalar();
            return (i) => { sv.Set(this.pin[i]); };
        }
    }

    public class Int2ShaderPin : AbstractValuePin<Vector2>
    {
        protected override void SetDefault(InputAttribute attr, EffectVariable var)
        {
            Vector4 vec = var.AsVector().GetVector();
            attr.DefaultValues = new double[] { vec.X, vec.Y };
            attr.AsInt = true;
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsVector();
            return (i) => { sv.Set(this.pin[i]); };
        }
    }

    public class Int3ShaderPin : AbstractValuePin<Vector3>
    {
        protected override void SetDefault(InputAttribute attr, EffectVariable var)
        {
            Vector4 vec = var.AsVector().GetVector();
            attr.DefaultValues = new double[] { vec.X, vec.Y, vec.Z };
            attr.AsInt = true;
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsVector();
            return (i) => { sv.Set(this.pin[i]); };
        }
    }

    public class Int4ShaderPin : AbstractValuePin<Vector4>
    {
        protected override void SetDefault(InputAttribute attr, EffectVariable var)
        {
            Vector4 vec = var.AsVector().GetVector();
            attr.DefaultValues = new double[] { vec.X, vec.Y, vec.Z, vec.W };
            attr.AsInt = true;
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsVector();
            return (i) => { sv.Set(this.pin[i]); };
        }
    }
}
