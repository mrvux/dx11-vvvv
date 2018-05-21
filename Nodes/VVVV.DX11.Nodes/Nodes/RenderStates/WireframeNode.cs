using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;

using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Wireframe", Category = "DX11.RenderState",Version="", Author = "vux", Help ="Modifies current ratserizer state to enable or disable wireframe, without chaging the rest of render state")]
    public class WireframeNode : IPluginEvaluate
    {
        [Input("Render State", CheckIfChanged = true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Enabled",DefaultValue=1)]
        protected IDiffSpread<bool> enabled;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

        public void Evaluate(int SpreadMax)
        {
            if (this.enabled.IsChanged
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

                    RasterizerStateDescription bs = rs.Rasterizer;
                    bs.FillMode = this.enabled[i] ? FillMode.Wireframe : FillMode.Solid;
                    rs.Rasterizer = bs;

                    this.FOutState[i] = rs;
                }

            }

        }
    }
}
