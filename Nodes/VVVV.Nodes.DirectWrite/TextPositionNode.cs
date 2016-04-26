using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using SlimDX;
using FeralTic.Core.Maths;
using SlimDX.DirectWrite;

using DWriteFactory = SlimDX.DirectWrite.Factory;
using System.ComponentModel.Composition;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "CaretPosition", Category = "String", Version = "DirectWrite", Author = "vux")]
    public class CaretPositionNode : IPluginEvaluate
    {
        [Input("Text Layout",CheckIfChanged=true)]
        protected Pin<TextLayout> FLayout;

        [Input("Index")]
        protected IDiffSpread<int> FIndex;

        [Input("Trailing")]
        protected IDiffSpread<bool> FTrailing;

        [Output("Position")]
        protected ISpread<Vector2> FPosition;

        public void Evaluate(int SpreadMax)
        {
            if (!FLayout.PluginIO.IsConnected)
            {
                this.FPosition.SliceCount = 0;
                return;
            }

            if (this.FLayout.IsChanged|| this.FIndex.IsChanged || this.FTrailing.IsChanged)
            {
                this.FPosition.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    TextLayout layout = this.FLayout[i];
                    float x,y;
                    var result = layout.HitTestTextPosition(this.FIndex[i], this.FTrailing[i], out x, out y);
                    this.FPosition[i] = new Vector2(x, y);
                }
            }
        }
    }
}
