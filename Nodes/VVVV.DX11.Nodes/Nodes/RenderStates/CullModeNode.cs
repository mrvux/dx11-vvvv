using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;

using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "CullMode", Category = "DX11.RenderState",Version="", Author = "vux", Help ="Modifies current ratserizer state cull mode, without chaging the rest of render state")]
    public class CullModeNode : IPluginEvaluate
    {
        [Input("Render State", CheckIfChanged = true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Mode")]
        protected IDiffSpread<CullMode> mode;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

        public void Evaluate(int SpreadMax)
        {
            if (this.mode.IsChanged
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
                    bs.CullMode = this.mode[i];
                    rs.Rasterizer = bs;

                    this.FOutState[i] = rs;
                }

            }

        }
    }
}
