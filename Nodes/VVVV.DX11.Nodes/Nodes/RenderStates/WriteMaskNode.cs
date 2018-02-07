using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;

using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "WriteMask", Category = "DX11.RenderState",Version="", Author = "vux", Help ="Modifies current blend state to only apply the specified color channels to the write mask")]
    public class WriteMaskNode : IPluginEvaluate
    {
        [Input("Render State", CheckIfChanged = true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Red",DefaultValue=1)]
        protected IDiffSpread<bool> red;

        [Input("Green", DefaultValue = 1)]
        protected IDiffSpread<bool> green;

        [Input("Blue", DefaultValue = 1)]
        protected IDiffSpread<bool> blue;

        [Input("Alpha", DefaultValue = 1)]
        protected IDiffSpread<bool> alpha;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

        public void Evaluate(int SpreadMax)
        {
            if (this.red.IsChanged
                || this.green.IsChanged
                || this.blue.IsChanged
                || this.alpha.IsChanged
                || this.FInState.IsChanged)
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

                    ColorWriteMaskFlags flag = ColorWriteMaskFlags.None;
                    if (red[i])
                        flag |= ColorWriteMaskFlags.Red;
                    if (green[i])
                        flag |= ColorWriteMaskFlags.Green;
                    if (blue[i])
                        flag |= ColorWriteMaskFlags.Blue;
                    if (alpha[i])
                        flag |= ColorWriteMaskFlags.Alpha;

                    BlendStateDescription bs = rs.Blend;
                    bs.RenderTargets[0].RenderTargetWriteMask = flag;


                    rs.Blend = bs;

                    this.FOutState[i] = rs;
                }

            }

        }
    }
}
