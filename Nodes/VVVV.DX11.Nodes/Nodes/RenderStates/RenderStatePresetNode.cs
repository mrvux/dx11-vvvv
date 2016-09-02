using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;
using VVVV.Utils.VColor;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "RenderState", Category = "DX11", Version = "", Author = "vux")]
    public class DX11RenderStatePresetNode : IPluginEvaluate
    {
        protected IDiffSpread<EnumEntry> FInBlendPreset;
        protected IDiffSpread<EnumEntry> FInDepthPreset;
        protected IDiffSpread<EnumEntry> FInRasterPreset;

        [Input("Stencil Reference Value")]
        protected IDiffSpread<int> FInStencilReference;

        [Input("Blend Factor", DefaultColor = new double[] { 0, 0, 0, 0 })]
        protected IDiffSpread<Color4> FInBlendFactor;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

        [ImportingConstructor()]
        public DX11RenderStatePresetNode(IPluginHost host, IIOFactory iofactory)
        {
            InputAttribute attr = new InputAttribute("Blend Mode");
            attr.EnumName = DX11BlendStates.Instance.EnumName;
            this.FInBlendPreset = iofactory.CreateDiffSpread<EnumEntry>(attr);

            attr = new InputAttribute("Rasterizer Mode");
            attr.EnumName = DX11RasterizerStates.Instance.EnumName;
            this.FInRasterPreset = iofactory.CreateDiffSpread<EnumEntry>(attr);


            attr = new InputAttribute("Depth Stencil Mode");
            attr.EnumName = DX11DepthStencilStates.Instance.EnumName;
            this.FInDepthPreset = iofactory.CreateDiffSpread<EnumEntry>(attr);
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInBlendPreset.IsChanged
                || this.FInDepthPreset.IsChanged
                || this.FInRasterPreset.IsChanged
                || this.FInStencilReference.IsChanged)
            {
                this.FOutState.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FOutState[i] = new DX11RenderState()
                    {
                        Blend = DX11BlendStates.Instance.GetState(this.FInBlendPreset[i].Name),
                        DepthStencil = DX11DepthStencilStates.Instance.GetState(this.FInDepthPreset[i].Name),
                        Rasterizer = DX11RasterizerStates.Instance.GetState(this.FInRasterPreset[i].Name),
                        DepthStencilReference = FInStencilReference[i],
                        BlendFactor = FInBlendFactor[i]
                    };
                }
            }
        }
    }
}


