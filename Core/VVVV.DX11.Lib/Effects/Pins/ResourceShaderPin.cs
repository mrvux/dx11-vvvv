using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using VVVV.DX11.Lib.Effects.Pins;
using VVVV.DX11;
using VVVV.PluginInterfaces.V2;
using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public abstract class ResourceShaderPin<U, R> : AbstractShaderV2Pin<DX11Resource<U>>
        where R : Resource
        where U : IDX11Resource
    {
        protected abstract ShaderResourceView GetSRV(DX11RenderContext context, int slice);

        protected virtual ShaderResourceView GetDefaultSRV(DX11RenderContext context) { return null; }


        protected override void ProcessAttribute(InputAttribute attr, EffectVariable var)
        {
            //Do nothing
        }

        protected override bool RecreatePin(EffectVariable variable)
        {
            return false;
        }

        private ShaderResourceView GetView(DX11ShaderInstance shaderinstance, int slice)
        {
            if (this.pin.IsConnected)
            {
                return this.GetSRV(shaderinstance.RenderContext, slice);
            }
            else
            {
                return this.GetDefaultSRV(shaderinstance.RenderContext);
            }
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsResource();
            return (i) => { sv.SetResource(this.GetView(instance, i)); };
        }
    }
}
