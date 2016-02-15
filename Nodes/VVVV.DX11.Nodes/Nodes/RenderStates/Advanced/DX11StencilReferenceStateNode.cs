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
    [PluginInfo(Name = "StencilReference", Category = "DX11.RenderState",Version="", Author = "vux")]
    public class DX11StencilReferencerStateNode : IPluginEvaluate
    {
        [Input("Render State", CheckIfChanged = true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Reference Value")]
        protected IDiffSpread<int> FInReference;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInState.IsChanged || this.FInReference.IsChanged)
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

                    rs.DepthStencilReference = this.FInReference[i];
                    this.FOutState[i] = rs;
                }

            }

        }
    }
}
