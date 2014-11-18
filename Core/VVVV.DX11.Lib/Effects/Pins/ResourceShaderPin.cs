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

        public override void SetVariable(DX11ShaderInstance shaderinstance, int slice)
        {
            if (this.pin.PluginIO.IsConnected)
            {
                shaderinstance.SetByName(this.Name, this.GetSRV(shaderinstance.RenderContext, slice));
            }
            else
            {
                shaderinstance.SetByName(this.Name, this.GetDefaultSRV(shaderinstance.RenderContext));
            }
        }
    }
}
