using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System.ComponentModel.Composition;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "StencilArrayView", Category = "DX11.Texture", Version = "2d",
        AutoEvaluate = true,
        Author = "vux",
        Warnings = "")]
    public class StencilTextureArrayNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Depth Stencil In")]
        protected Pin<DX11Resource<DX11DepthTextureArray>> FTextureInput;

        [Output("Texture Out")]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureOutput;

        [ImportingConstructor()]
        public StencilTextureArrayNode(IHDEHost hde)
        {

        }

        public void Evaluate(int SpreadMax)
        {
            this.FTextureOutput.SliceCount = SpreadMax;

            for (int i = 0; i < this.FTextureOutput.SliceCount; i++)
            {
                if (this.FTextureOutput[i] == null)
                {
                    this.FTextureOutput[i] = new DX11Resource<DX11Texture2D>();
                }
            }
        }

        public void Update(DX11RenderContext context)
        {
            for (int i = 0; i < this.FTextureOutput.SliceCount; i++)
            {
                if (this.FTextureInput[i].Contains(context) && this.FTextureInput[i][context] != null)
                {
                    this.FTextureOutput[i][context] = this.FTextureInput[i][context].Stencil;
                }
                else
                {
                    this.FTextureOutput[i].Remove(context);
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }
    }
}
