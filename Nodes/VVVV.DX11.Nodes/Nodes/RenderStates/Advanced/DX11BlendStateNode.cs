using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;

using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Blend", Category = "DX11.RenderState",Version="Advanced", Author = "vux,tonfilm")]
    public class DX11BlendStateNode : IPluginEvaluate
    {
        [Input("Render State", CheckIfChanged = true)]
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

        [Output("Render State")]
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

                for (int i = 0; i < SpreadMax; i++)
                {
                    DX11RenderState rs;
                    if (this.FInState.PluginIO.IsConnected)
                    {
                        rs = this.FInState[i].Clone();
                    }
                    else
                    {
                        rs = new DX11RenderState();
                    }

                    BlendStateDescription bs = rs.Blend;
                    bs.AlphaToCoverageEnable = this.FInAlphaCover[i];
                    bs.RenderTargets[0].BlendEnable = this.FInEnable[i];
                    bs.RenderTargets[0].BlendOperation = this.FInBlendOp[i];
                    bs.RenderTargets[0].BlendOperationAlpha = this.FInBlendOpAlpha[i];
                    bs.RenderTargets[0].RenderTargetWriteMask = this.FInWriteMask[i];
                    bs.RenderTargets[0].SourceBlend = this.FInSrc[i];
                    bs.RenderTargets[0].SourceBlendAlpha = this.FInSrcAlpha[i];
                    bs.RenderTargets[0].DestinationBlend = this.FInDest[i];
                    bs.RenderTargets[0].DestinationBlendAlpha = this.FInDestAlpha[i];


                    rs.Blend = bs;

                    this.FOutState[i] = rs;
                }

            }

        }
    }
}
