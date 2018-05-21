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
    [PluginInfo(Name = "EmptyTexture", Category = "DX11.Texture", Version = "2d",
        AutoEvaluate = true,
        Author = "vux",
        Warnings = "This node is used by test patches, it is useless for production",
        Help ="Deploys an empty resource dictionary")]
    public class EmptyTexture2dTestNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Output("Texture Out", IsSingle = true)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureOutput;

        public void Evaluate(int SpreadMax)
        {
            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11Texture2D>();
            }
        }

        public void Update(DX11RenderContext context)
        {

        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }
    }
}
