using FeralTic.DX11;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.Lib.Effects.TextureFX
{
    public class ImageShaderInfo
    {
        private ImageShaderTechniqueInfo[] techniqueInfo;

        private List<EffectResourceVariable> depthTextureVariables = new List<EffectResourceVariable>();
        private List<EffectResourceVariable> initialTextureVariables = new List<EffectResourceVariable>();
        private List<EffectResourceVariable> previousTextureVariables = new List<EffectResourceVariable>();
        

        public ImageShaderInfo(DX11ShaderInstance shader)
        {
            var effect = shader.Effect;
            this.techniqueInfo = new ImageShaderTechniqueInfo[effect.Description.TechniqueCount];

            for (int i = 0; i < this.techniqueInfo.Length; i++)
            {
                this.techniqueInfo[i] = new ImageShaderTechniqueInfo(effect.GetTechniqueByIndex(i));
            }

            for (int i = 0; i < effect.Description.GlobalVariableCount; i++)
            {
                EffectVariable var = effect.GetVariableByIndex(i);

                if (var.GetVariableType().Description.TypeName == "Texture2D")
                {
                    EffectResourceVariable rv = var.AsResource();

                    if (rv.Description.Semantic == "INITIAL")
                    {
                        this.initialTextureVariables.Add(rv);
                    }
                    if (rv.Description.Semantic == "PREVIOUS")
                    {
                        this.previousTextureVariables.Add(rv);
                    }
                    if (rv.Description.Semantic == "DEPTHTEXTURE")
                    {
                        this.depthTextureVariables.Add(rv);
                    }
                }
            }
        }

        public int TerchniqueCount => this.techniqueInfo.Length;

        public ImageShaderTechniqueInfo GetTechniqueInfo(int index)
        {
            return this.techniqueInfo[index];
        }

        public void ApplyInitial(ShaderResourceView view)
        {
            for (int i = 0; i < this.initialTextureVariables.Count; i++)
            {
                this.initialTextureVariables[i].SetResource(view);
            }
        }

        public void ApplyPrevious(ShaderResourceView view)
        {
            for (int i = 0; i < this.previousTextureVariables.Count; i++)
            {
                this.previousTextureVariables[i].SetResource(view);
            }
        }

        public void ApplyDepth(ShaderResourceView view)
        {
            for (int i = 0; i < this.depthTextureVariables.Count; i++)
            {
                this.depthTextureVariables[i].SetResource(view);
            }
        }
    }
}
