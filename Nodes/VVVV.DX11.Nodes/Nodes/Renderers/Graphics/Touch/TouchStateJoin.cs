using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX;

namespace VVVV.DX11.Nodes.Renderers.Graphics.Touch
{
    [PluginInfo(Name="TouchState",Category ="System",Version="Join",Author="vux,woei")]
    public class EncodeTouchDataNode : IPluginEvaluate
    {
        [Input("Id")]
        protected ISpread<int> FId;

        [Input("Position")]
        protected ISpread<Vector2> FPos;

        [Input("Is New")]
        protected ISpread<bool> FNew;

        [Output("Touch Data")]
        protected ISpread<TouchData> FData;

        public void Evaluate(int SpreadMax)
        {
            this.FData.SliceCount = SpreadMax;

            var buffer = this.FData.Stream.Buffer;

            for (int i = 0; i < SpreadMax; i++ )
            {
                TouchData td = new TouchData()
                {
                    Id = FId[i],
                    IsNew = FNew[i],
                    Pos = FPos[i]
                };
                FData[i] = td;
            }
        }
    }
}
