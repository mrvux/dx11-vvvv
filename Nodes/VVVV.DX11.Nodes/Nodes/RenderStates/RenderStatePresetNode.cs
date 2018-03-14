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
        [Input("Blend Mode")]
        protected IDiffSpread<BlendStatePreset> FBlendMode;

        [Input("Rasterizer Mode")]
        protected IDiffSpread<RasterizerStatePreset> FRasterMode;

        [Input("Depth Stencil Mode")]
        protected IDiffSpread<DepthStencilStatePreset> FDepthMode;

        [Input("Stencil Reference Value")]
        protected IDiffSpread<int> FInStencilReference;

        [Input("Blend Factor", DefaultColor = new double[] { 0, 0, 0, 0 })]
        protected IDiffSpread<Color4> FInBlendFactor;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;


        public void Evaluate(int SpreadMax)
        {
            if (this.FBlendMode.IsChanged
                || this.FDepthMode.IsChanged
                || this.FRasterMode.IsChanged
                || this.FInStencilReference.IsChanged)
            {
                this.FOutState.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FOutState[i] = new DX11RenderState()
                    {
                        Blend = DX11BlendStates.GetState(this.FBlendMode[i]),
                        DepthStencil = DX11DepthStencilStates.GetState(this.FDepthMode[i]),
                        Rasterizer = DX11RasterizerStates.GetState(this.FRasterMode[i]),
                        DepthStencilReference = FInStencilReference[i],
                        BlendFactor = FInBlendFactor[i]
                    };
                }
            }
        }
    }
}


