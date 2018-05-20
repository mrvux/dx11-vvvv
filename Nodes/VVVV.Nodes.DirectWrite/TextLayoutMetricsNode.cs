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
    [PluginInfo(Name = "TextLayoutMetrics", Category = "DirectWrite", Version = "", Author = "vux")]
    public class TextLayoutMetricsNode : IPluginEvaluate
    {
        [Input("Text Layout", CheckIfChanged = true)]
        protected Pin<TextLayout> FInText;

        [Output("Left")]
        protected ISpread<float> FLeft;

        [Output("Top")]
        protected ISpread<float> FTop;

        [Output("Width")]
        protected ISpread<float> FWidth;

        [Output("Height")]
        protected ISpread<float> FHeight;

        [Output("Line Count")]
        protected ISpread<int> FLineCount;

        private DWriteFactory dwFactory;

        [ImportingConstructor()]
        public TextLayoutMetricsNode(DWriteFactory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInText.IsConnected == false)
            {
                this.FLeft.SliceCount = 0;
                this.FWidth.SliceCount = 0;
                this.FTop.SliceCount = 0;
                this.FHeight.SliceCount = 0;
                this.FLineCount.SliceCount = 0;
                return;
            }

            if (this.FInText.IsChanged)
            {
                this.FLeft.SliceCount = SpreadMax;
                this.FTop.SliceCount = SpreadMax;
                this.FWidth.SliceCount = SpreadMax;
                this.FHeight.SliceCount = SpreadMax;
                this.FLineCount.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax;i++)
                {
                    var metrics = this.FInText[i].Metrics;
                    this.FLeft[i] = metrics.Left;
                    this.FTop[i] = metrics.Top;
                    this.FWidth[i] = metrics.Width;
                    this.FHeight[i] = metrics.Height;
                    this.FLineCount[i] = metrics.LineCount;
                }

            }
        }
    }
}
