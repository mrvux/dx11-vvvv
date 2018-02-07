using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;

using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "AlphaOperation", Category = "DX11.RenderState",Version="", Author = "vux", Help ="Modifies current blend state to modify in which way alpha channel is written")]
    public class AlphaOperationNode : IPluginEvaluate
    {
        public enum AlphaOperationMode
        {
            Replace,
            Keep,
            Multiply,
            Interpolate,
        }

        [Input("Render State", CheckIfChanged = true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Mode")]
        protected IDiffSpread<AlphaOperationMode> mode;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

        public void Evaluate(int SpreadMax)
        {
            if (this.mode.IsChanged || this.FInState.IsChanged)
            {
                this.FOutState.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    DX11RenderState rs;
                    if (this.FInState.IsConnected)
                    {
                        rs = this.FInState[i].Clone();
                    }
                    else
                    {
                        rs = new DX11RenderState();
                    }

                    BlendStateDescription bs = rs.Blend;
                    RenderTargetBlendDescription target = bs.RenderTargets[0];

                    //Note: to optimize a little, we do the following : 
                    //In any case, if blend is disabled, we enable it as otherwise modes with do nothing
                    //Only exception is replace, if blend is disabled we leave as it is

                    switch (mode[i])
                    {
                        case AlphaOperationMode.Keep:
                            target.BlendEnable = true;
                            target.BlendOperation = BlendOperation.Add;
                            target.DestinationBlendAlpha = BlendOption.One;
                            target.SourceBlendAlpha = BlendOption.Zero;
                            break;
                        case AlphaOperationMode.Replace:
                            if (target.BlendEnable)
                            {
                                target.BlendOperation = BlendOperation.Add;
                                target.DestinationBlendAlpha = BlendOption.Zero;
                                target.SourceBlendAlpha = BlendOption.One;
                            }
                            break;
                        case AlphaOperationMode.Multiply:
                            target.BlendEnable = true;
                            target.BlendOperation = BlendOperation.Add;
                            target.DestinationBlendAlpha = BlendOption.SourceAlpha;
                            target.SourceBlendAlpha = BlendOption.Zero;
                            break;
                        case AlphaOperationMode.Interpolate:
                            target.BlendEnable = true;
                            target.BlendOperation = BlendOperation.Add;
                            target.DestinationBlendAlpha = BlendOption.InverseSourceAlpha;
                            target.SourceBlendAlpha = BlendOption.SourceAlpha;
                            break;
                    }

                    bs.RenderTargets[0] = target;
                    rs.Blend = bs;

                    this.FOutState[i] = rs;
                }

            }

        }
    }
}
