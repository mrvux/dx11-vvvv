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
    [PluginInfo(Name = "Tag", Category = "DX11.RenderState",Version="", Author = "vux")]
    public class DX11TagStateNode : IPluginEvaluate
    {
        [Input("Render State", CheckIfChanged = true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Tag")]
        protected IDiffSpread<object> FInTag;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInState.IsChanged || this.FInTag.IsChanged)
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

                    rs.Tag = this.FInTag[i];
                    this.FOutState[i] = rs;
                }

            }

        }
    }
}
