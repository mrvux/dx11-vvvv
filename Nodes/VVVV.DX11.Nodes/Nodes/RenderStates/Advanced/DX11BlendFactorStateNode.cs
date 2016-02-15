using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;

using FeralTic.DX11;
using SlimDX;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "BlendFactor", Category = "DX11.RenderState",Version="", Author = "vux")]
    public class DX11BlendFactorStateNode : IPluginEvaluate
    {
        [Input("Render State", CheckIfChanged = true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Blend Factor", DefaultColor= new double[] {0,0,0,0})]
        protected IDiffSpread<Color4> FInFactor;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInFactor.IsChanged || this.FInState.IsChanged)
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

                    rs.BlendFactor = this.FInFactor[i];
                    this.FOutState[i] = rs;
                }

            }

        }
    }
}
