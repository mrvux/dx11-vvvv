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
    [PluginInfo(Name = "UpdateFlag", Category = "DX11.Texture", Version = "2d",
        AutoEvaluate = true,
        Author = "vux",
        Warnings = "This node is used by test patches, it is useless for production",
        Help ="Checks if texture is updated or now in a flag")]
    public class UpdateFlag2dTestNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Output("Is Updated", IsSingle = true)]
        protected ISpread<bool> FUpdated;

        [Output("Texture Out", IsSingle = true)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureOutput;

        bool lastUpdate = false;

        public void Evaluate(int SpreadMax)
        {
            this.FUpdated[0] = lastUpdate;
            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11Texture2D>();
            }

            this.lastUpdate = false;
        }

        public void Update(DX11RenderContext context)
        {
            this.lastUpdate = true;
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }
    }
}
