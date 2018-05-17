using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.Lib.Effects.TextureFX
{
    public class ImageShaderTechniqueInfo
    {
        private ImageShaderPass[] passInfo;

        public ImageShaderTechniqueInfo(EffectTechnique technique)
        {
            this.passInfo = new ImageShaderPass[technique.Description.PassCount];

            for (int i = 0; i < this.passInfo.Length; i++)
            {
                this.passInfo[i] = new ImageShaderPass(technique.GetPassByIndex(i));
            }
        }

        public int PassCount => this.passInfo.Length;

        public ImageShaderPass GetPassInfo(int index)
        {
            return this.passInfo[index];
        }

    }
}
