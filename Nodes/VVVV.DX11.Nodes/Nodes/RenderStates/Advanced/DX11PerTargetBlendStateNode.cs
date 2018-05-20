using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;

using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "BlendTarget", Category = "DX11.RenderState",Version="Advanced", Author = "vux")]
    public class DX11PerTargetBlendStateNode : IPluginEvaluate
    {
        [Input("Render State", CheckIfChanged = true, IsSingle =true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Alpha To Coverage",DefaultValue=0)]
        protected IDiffSpread<bool> FInAlphaCover;

        [Input("Enabled", DefaultValue = 0)]
        protected IDiffSpread<bool> FInEnable;

        [Input("Operation", DefaultEnumEntry = "Maximum")]
        protected IDiffSpread<BlendOperation> FInBlendOp;

        [Input("Alpha Operation", DefaultEnumEntry = "Maximum")]
        protected IDiffSpread<BlendOperation> FInBlendOpAlpha;

        [Input("Source Blend", DefaultEnumEntry = "One")]
        protected IDiffSpread<BlendOption> FInSrc;

        [Input("Source Alpha Blend", DefaultEnumEntry = "One")]
        protected IDiffSpread<BlendOption> FInSrcAlpha;

        [Input("Destination Blend", DefaultEnumEntry = "Zero")]
        protected IDiffSpread<BlendOption> FInDest;

        [Input("Destination Alpha Blend", DefaultEnumEntry = "Zero")]
        protected IDiffSpread<BlendOption> FInDestAlpha;

        [Input("Write Mask", DefaultEnumEntry = "All")]
        protected IDiffSpread<ColorWriteMaskFlags> FInWriteMask;

        [Output("Render State", IsSingle =true)]
        protected ISpread<DX11RenderState> FOutState;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInAlphaCover.IsChanged
                || this.FInEnable.IsChanged
                || this.FInBlendOp.IsChanged
                || this.FInBlendOpAlpha.IsChanged
                || this.FInWriteMask.IsChanged
                || this.FInSrc.IsChanged
                || this.FInSrcAlpha.IsChanged
                || this.FInDest.IsChanged
                || this.FInDestAlpha.IsChanged)
            {
                this.FOutState.SliceCount = SpreadMax;

                DX11RenderState rs;
                if (this.FInState.IsConnected)
                {
                    rs = this.FInState[0].Clone();
                }
                else
                {
                    rs = new DX11RenderState();
                }

                BlendStateDescription bs = rs.Blend;

                for (int i = 0; i < 8; i++)
                {
                    bs.IndependentBlendEnable = true;
                    bs.AlphaToCoverageEnable = this.FInAlphaCover[0];
                    bs.RenderTargets[i].BlendEnable = this.FInEnable[i];
                    bs.RenderTargets[i].BlendOperation = this.FInBlendOp[i];
                    bs.RenderTargets[i].BlendOperationAlpha = this.FInBlendOpAlpha[i];
                    bs.RenderTargets[i].RenderTargetWriteMask = this.FInWriteMask[i];
                    bs.RenderTargets[i].SourceBlend = this.FInSrc[i];
                    bs.RenderTargets[i].SourceBlendAlpha = this.FInSrcAlpha[i];
                    bs.RenderTargets[i].DestinationBlend = this.FInDest[i];
                    bs.RenderTargets[i].DestinationBlendAlpha = this.FInDestAlpha[i];   
                }

                rs.Blend = bs;

                this.FOutState[0] = rs;
            }

        }
    }
}
