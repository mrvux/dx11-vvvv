using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D11;

using VVVV.Hosting.Pins;
using VVVV.DX11.Lib;
using VVVV.DX11.Lib.Effects.Pins;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using SlimDX;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public class Texture3DShaderPin : AbstractShaderV2Pin<DX11Resource<DX11Texture3D>>
    {
        protected override void ProcessAttribute(InputAttribute attr, EffectVariable var)
        {
            //Do nothing
        }

        protected override bool RecreatePin(EffectVariable variable)
        {
            return false;
        }

        private DX11Texture3D GetResource(DX11RenderContext context, int slice)
        {
            if (this.pin[slice] != null && this.pin[slice].Contains(context))
            {
                return this.pin[slice][context];
            }
            else
            {
                return null;
            }
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsResource();

            List<EffectVectorVariable> sizeOfVar = new List<EffectVectorVariable>();
            List<EffectVectorVariable> invSizeOfVar = new List<EffectVectorVariable>();

            for (int i = 0; i < instance.Effect.Description.GlobalVariableCount; i++)
            {
                var v = instance.Effect.GetVariableByIndex(i);
                if (v.GetVariableType().Description.TypeName == "float3" && v.Description.Semantic == "SIZEOF" && v.Reference(this.Name))
                {
                    sizeOfVar.Add(v.AsVector());
                }
                if (v.GetVariableType().Description.TypeName == "float3" && v.Description.Semantic == "INVSIZEOF" && v.Reference(this.Name))
                {
                    invSizeOfVar.Add(v.AsVector());
                }
            }
            if (sizeOfVar.Count == 0 && invSizeOfVar.Count == 0)
            {
                return (i) => 
                {
                    var res = this.GetResource(instance.RenderContext, i);
                    sv.SetResource(res != null ? res.SRV : null);
                };
            }
            else
            {
                return (i) =>
                {
                    var resource = this.GetResource(instance.RenderContext, i);
                    sv.SetResource(resource != null ? resource.SRV : null);

                    if (resource != null)
                    {
                        for (int j = 0; j < sizeOfVar.Count; j++)
                        {
                            sizeOfVar[j].Set(new Vector3(resource.Width, resource.Height, resource.Depth));
                        }
                        for (int j = 0; j < invSizeOfVar.Count; j++)
                        {
                            invSizeOfVar[j].Set(new Vector3(1.0f / resource.Width, 1.0f / resource.Height, 1.0f / resource.Depth));
                        }
                    }
                    else
                    {
                        for (int j = 0; j < sizeOfVar.Count; j++)
                        {
                            sizeOfVar[j].Set(new Vector3(1, 1, 1));
                        }
                        for (int j = 0; j < invSizeOfVar.Count; j++)
                        {
                            invSizeOfVar[j].Set(new Vector3(1, 1, 1));
                        }
                    }
                };
            }
        }
    }
}
