using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX;

namespace VVVV.DX11.Nodes.Renderers.Graphics.Touch
{
    [PluginInfo(Name="TouchState",Category ="System",Version="Split",Author="vux,woei")]
    public class DecodeTouchDataNode : IPluginEvaluate
    {
        [Input("Touch Data")]
        Pin<TouchData> FInData;

        [Output("Id")]
        ISpread<int> FTouchId;

        [OutputAttribute("Position")]
        ISpread<Vector2> FOutPos;

        [OutputAttribute("Is New")]
        ISpread<bool> FOutNew;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInData.PluginIO.IsConnected)
            {
                this.FTouchId.SliceCount = this.FInData.SliceCount;
                this.FOutPos.SliceCount = this.FInData.SliceCount;
                this.FOutNew.SliceCount = this.FInData.SliceCount;

                for (int i = 0; i < this.FInData.SliceCount; i++)
                {
                    TouchData t = this.FInData[i];
                    this.FTouchId[i] = t.Id;
                    this.FOutNew[i] = t.IsNew;
                    this.FOutPos[i] = t.Pos;
                }
            }
            else
            {
                this.FTouchId.SliceCount = 0;
                this.FOutPos.SliceCount = 0;
                this.FOutNew.SliceCount = 0;
            }
        }
    }
}
