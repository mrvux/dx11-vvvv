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
    [PluginInfo(Name = "UpdateFlag", Category = "DX11.Layer", Version = "2d",
        AutoEvaluate = true,
        Author = "vux",
        Warnings = "This node is used by test patches, it is useless for production",
        Help ="Checks if texture is updated or now in a flag")]
    public class UpdateLayerFlag2dTestNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Output("Update Count", IsSingle = true)]
        protected ISpread<int> FUpdateCount;

        [Output("Render Count", IsSingle = true)]
        protected ISpread<int> FRenderCount;

        [Output("Texture Out", IsSingle = true)]
        protected Pin<DX11Resource<DX11Layer>> FTextureOutput;

        int lastUpdate = 0;
        int lastRender = 0;

        public void Evaluate(int SpreadMax)
        {
            this.FUpdateCount[0] = lastUpdate;
            this.FRenderCount[0] = lastRender;
            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11Layer>();
            }

            this.lastUpdate = 0;
            this.lastRender = 0;
        }

        public void Update(DX11RenderContext context)
        {
            this.lastUpdate++;
            if (!this.FTextureOutput[0].Contains(context))
            {
                DX11Layer layer = new DX11Layer();
                layer.Render = this.Render;
                this.FTextureOutput[0][context] = layer;
            }
               
        }

        private void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            this.lastRender++;
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }
    }
}
