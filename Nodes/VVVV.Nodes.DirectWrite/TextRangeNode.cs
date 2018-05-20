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
    [PluginInfo(Name = "CaretRange", Category = "String", Version = "DirectWrite", Author = "vux")]
    public class CaretRangeNode : IPluginEvaluate
    {
        [Input("Text Layout",CheckIfChanged=true)]
        protected Pin<TextLayout> FInLayout;

        [Input("Index")]
        protected IDiffSpread<int> FIndex;

        [Input("Range")]
        protected IDiffSpread<int> FRange;

        [Input("Trailing")]
        protected IDiffSpread<bool> FTrailing;


        [Output("Result Bin Size")]
        protected ISpread<float> FResultBin;

        [Output("Left")]
        protected ISpread<float> FLeft;

        [Output("Top")]
        protected ISpread<float> FTop;

        [Output("Width")]
        protected ISpread<float> FWidth;

        [Output("Height")]
        protected ISpread<float> FHeight;

        public void Evaluate(int SpreadMax)
        {
            if (!FInLayout.IsConnected)
            {
                this.FLeft.SliceCount = 0;
                this.FTop.SliceCount = 0;
                this.FWidth.SliceCount = 0;
                this.FHeight.SliceCount = 0;
                this.FResultBin.SliceCount = 0;
                return;
            }

            if (this.FInLayout.IsChanged || this.FIndex.IsChanged
                || this.FTrailing.IsChanged || this.FRange.IsChanged)
            {
                this.FLeft.SliceCount = SpreadMax;
                this.FTop.SliceCount = SpreadMax;
                this.FWidth.SliceCount = SpreadMax;
                this.FHeight.SliceCount = SpreadMax;
                this.FResultBin.SliceCount = SpreadMax;

                List<float> left = new List<float>();
                List<float> width = new List<float>();
                List<float> top = new List<float>();
                List<float> height = new List<float>();

                for (int i = 0; i < SpreadMax; i++)
                {
                    TextLayout layout = this.FInLayout[i];
                    var results = layout.HitTestTextRange(this.FIndex[i], this.FRange[i], 0.0f, 0.0f);

                    this.FResultBin[i] = results.Length;

                    left.AddRange(from r in results select r.Left);
                    width.AddRange(from r in results select r.Width);
                    top.AddRange(from r in results select r.Top);
                    height.AddRange(from r in results select r.Height);
                }

                this.FHeight.AssignFrom(height);
                this.FLeft.AssignFrom(left);
                this.FTop.AssignFrom(top);
                this.FWidth.AssignFrom(width);
            }
        }
    }
}
