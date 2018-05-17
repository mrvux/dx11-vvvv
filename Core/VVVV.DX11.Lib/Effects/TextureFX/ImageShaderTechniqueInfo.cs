using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.Lib.Effects
{
    public class ImageShaderTechniqueInfo
    {
        private ImageShaderPassInfo[] passInfo;

        public bool WantMips { get; private set; }

        public ImageShaderTechniqueInfo(EffectTechnique technique)
        {
            this.WantMips = technique.GetBoolTechniqueAnnotationByName("wantmips", false);

            this.passInfo = new ImageShaderPassInfo[technique.Description.PassCount];

            for (int i = 0; i < this.passInfo.Length; i++)
            {
                this.passInfo[i] = new ImageShaderPassInfo(technique.GetPassByIndex(i));
            }


        }

        public int PassCount => this.passInfo.Length;

        public ImageShaderPassInfo GetPassInfo(int index)
        {
            return this.passInfo[index];
        }

    }
}
