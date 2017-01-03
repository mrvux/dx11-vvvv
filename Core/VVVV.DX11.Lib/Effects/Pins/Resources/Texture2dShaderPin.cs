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
using FeralTic;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public class Texture2DShaderPin : AbstractShaderV2Pin<DX11Resource<DX11Texture2D>>
    {
        protected override void ProcessAttribute(InputAttribute attr, EffectVariable var)
        {
            //Do nothing
        }

        protected override bool RecreatePin(EffectVariable variable)
        {
            return false;
        }

        private DX11Texture2D GetResource(DX11RenderContext context, int slice)
        {
            if (this.pin[slice] != null && this.pin[slice].Contains(context))
            {
                var tex = this.pin[slice][context];
                return tex != null ? tex : context.DefaultTextures.BlackTexture;
            }
            else
            {
                return context.DefaultTextures.WhiteTexture;
            }
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsResource();

            List<EffectVectorVariable> sizeOfVar = new List<EffectVectorVariable>();
            List<EffectVectorVariable> invSizeOfVar = new List<EffectVectorVariable>();
            List<EffectMatrixVariable> aspectVar = new List<EffectMatrixVariable>();
            List<AspectRatioMode> aspectMode = new List<AspectRatioMode>();

            for (int i = 0; i < instance.Effect.Description.GlobalVariableCount; i++)
            {
                var v = instance.Effect.GetVariableByIndex(i);
                if (v.GetVariableType().Description.TypeName == "float2" && v.Description.Semantic == "SIZEOF" && v.Reference(this.Name))
                {
                    sizeOfVar.Add(v.AsVector());
                }
                if (v.GetVariableType().Description.TypeName == "float2" && v.Description.Semantic == "INVSIZEOF" && v.Reference(this.Name))
                {
                    invSizeOfVar.Add(v.AsVector());
                }
                if (v.GetVariableType().Description.TypeName == "float4x4" && v.Description.Semantic == "ASPECTOF" && v.Reference(this.Name))
                {
                    aspectVar.Add(v.AsMatrix());
                    aspectMode.Add(v.AspectMode());                   
                }
            }
            if (sizeOfVar.Count == 0 && invSizeOfVar.Count == 0 && aspectMode.Count == 0)
            {
                return (i) => { sv.SetResource(this.GetResource(instance.RenderContext, i).SRV); };
            }
            else
            {
                return (i) =>
                {
                    var resource = this.GetResource(instance.RenderContext, i);
                    sv.SetResource(resource.SRV);
                    for (int j = 0; j < sizeOfVar.Count; j++)
                    {
                        sizeOfVar[j].Set(new Vector2(resource.Width, resource.Height));
                    }
                    for (int j = 0; j < invSizeOfVar.Count; j++)
                    {
                        invSizeOfVar[j].Set(new Vector2(1.0f / resource.Width, 1.0f / resource.Height));
                    }
                    for (int j = 0; j < aspectVar.Count; j++)
                    {
                        aspectVar[j].SetMatrix(AspectUtils.AspectMatrix(new Vector2(resource.Width, resource.Height), aspectMode[i]));
                    }
                };
            }
        }
    }
}
