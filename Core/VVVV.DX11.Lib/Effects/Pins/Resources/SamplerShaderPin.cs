using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins;
using VVVV.DX11.Lib.Effects.Pins;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public class SamplerShaderPin : AbstractShaderV2Pin<SamplerDescription>
    {
        private SamplerState state;

        protected override void ProcessAttribute(InputAttribute attr, EffectVariable var)
        {
            attr.IsSingle = true;
            attr.CheckIfChanged = true;
        }

        protected override bool RecreatePin(EffectVariable var)
        {
            return false;
        }

        public override void SetVariable(DX11ShaderInstance shaderinstance, int slice)
        {
            if (this.pin.PluginIO.IsConnected)
            {
                using (var state = SamplerState.FromDescription(shaderinstance.RenderContext.Device, this.pin[slice]))
                {
                    shaderinstance.SetByName(this.Name, state);
                }
            }
            else
            {
                shaderinstance.Effect.GetVariableByName(this.Name).AsConstantBuffer().UndoSetConstantBuffer();
            }

            
        }
    }
}