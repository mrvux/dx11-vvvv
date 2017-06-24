using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;
using SlimDX.Direct3D11;
using VVVV.DX11.Lib;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Resources;
using FeralTic.DX11;


namespace VVVV.DX11.Lib.Effects.Pins.Resources
{
    public class ReadableStructuredBufferShaderPin : AbstractShaderV2Pin<DX11Resource<IDX11ReadableStructureBuffer>>
    {
        protected override void ProcessAttribute(InputAttribute attr, EffectVariable var)
        {
            //Do nothing
        }

        protected override bool RecreatePin(EffectVariable variable)
        {
            return false;
        }

        private IDX11ReadableStructureBuffer GetResource(DX11RenderContext context, int slice)
        {
            if (this.pin[slice] != null && this.pin[slice].Contains(context))
            {
                IDX11ReadableStructureBuffer sb = this.pin[slice][context];
                return sb;
            }
            else
            {
                return null;
            }
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsResource();

            List<EffectScalarVariable> sizeOfVar = new List<EffectScalarVariable>();

            for (int i = 0; i < instance.Effect.Description.GlobalVariableCount; i++)
            {
                var v = instance.Effect.GetVariableByIndex(i);
                if (v.GetVariableType().Description.TypeName == "uint" && v.Description.Semantic == "SIZEOF" && v.Reference(this.Name))
                {
                    sizeOfVar.Add(v.AsScalar());
                }
            }
            if (sizeOfVar.Count == 0)
            {
                return (i) => 
                {
                    var resource = this.GetResource(instance.RenderContext, i);
                    sv.SetResource(resource != null ? resource.SRV : null);
                };
            }
            else
            {
                return (i) =>
                {
                    var resource = this.GetResource(instance.RenderContext, i);
                    sv.SetResource(resource != null ? resource.SRV : null);
                    for (int j = 0; j < sizeOfVar.Count; j++)
                    {
                        sizeOfVar[j].Set(resource != null ? resource.ElementCount : 0);
                    }
                };
            }
        }
    }

    public class ReadableBufferShaderPin : ResourceShaderPin<IDX11ReadableResource, SlimDX.Direct3D11.Buffer>
    {
        protected override ShaderResourceView GetSRV(DX11RenderContext context, int slice)
        {
            if (this.pin[slice] == null)
            {
                return null;
            }
            else
            {
                if (!this.pin[slice].Contains(context))
                {
                    return null;
                }
                else
                {
                    IDX11ReadableResource sb = this.pin[slice][context];
                    return sb != null ? sb.SRV : null;
                }
            }
        }
    }
}
