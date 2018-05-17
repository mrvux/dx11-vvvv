using FeralTic.DX11;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.Lib.Effects
{
    public class ImageShaderInfo
    {
        private ImageShaderTechniqueInfo[] techniqueInfo;

        private List<EffectResourceVariable> depthTextureVariables = new List<EffectResourceVariable>();
        private List<EffectResourceVariable> initialTextureVariables = new List<EffectResourceVariable>();
        private List<EffectResourceVariable> previousTextureVariables = new List<EffectResourceVariable>();

        private List<EffectResourceVariable>[] passResultVariableArray;

        public ImageShaderInfo(DX11ShaderInstance shader)
        {
            var effect = shader.Effect;
            this.techniqueInfo = new ImageShaderTechniqueInfo[effect.Description.TechniqueCount];

            int maxPassCount = 0;

            for (int i = 0; i < this.techniqueInfo.Length; i++)
            {
                this.techniqueInfo[i] = new ImageShaderTechniqueInfo(effect.GetTechniqueByIndex(i));
                maxPassCount = this.techniqueInfo[i].PassCount > maxPassCount ? this.techniqueInfo[i].PassCount : maxPassCount;
            }

            //Set list of max pass count
            this.passResultVariableArray = new List<EffectResourceVariable>[maxPassCount];
            

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

                    //If semantic starts with passresult
                    if (rv.Description.Semantic.StartsWith("PASSRESULT"))
                    {
                        string sidx = rv.Description.Semantic.Substring(10);
                        int pridx;
                        if (int.TryParse(sidx, out pridx))
                        {
                            if (pridx < maxPassCount)
                            {
                                if (this.passResultVariableArray[pridx] == null)
                                {
                                    this.passResultVariableArray[pridx] = new List<EffectResourceVariable>();
                                }
                                this.passResultVariableArray[pridx].Add(rv);
                            }
                        }
                    }
                }
            }


        }

        public int TerchniqueCount => this.techniqueInfo.Length;

        public ImageShaderTechniqueInfo GetTechniqueInfo(int index)
        {
            return this.techniqueInfo[index];
        }

        public void ApplyPassResult(ShaderResourceView view, int passIndex)
        {
            var prData = this.passResultVariableArray[passIndex];
            if (prData != null)
            {
                for (int i = 0; i < prData.Count; i++)
                {
                    prData[i].SetResource(view);
                }
            }

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
