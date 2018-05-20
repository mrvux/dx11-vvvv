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
    [PluginInfo(Name = "LineMetrics", Category = "DirectWrite", Version = "TextLayout", Author = "vux")]
    public class TextLayoutLineMetricsNode : IPluginEvaluate
    {
        [Input("Text Layout", CheckIfChanged = true)]
        protected Pin<TextLayout> FInText;

        [Output("Line Metrics Count")]
        protected ISpread<int> metricsCount;

        [Output("Length")]
        protected ISpread<int> length;

        [Output("Trailing Whitespace Length")]
        protected ISpread<int> trailingWhitespaceLength;

        [Output("New Line Length")]
        protected ISpread<int> newlineLength;

        [Output("Height")]
        protected ISpread<float> height;

        [Output("Baseline")]
        protected ISpread<float> baseline;

        [Output("Is Trimmed")]
        protected ISpread<bool> isTrimmed;

        private DWriteFactory dwFactory;
        private List<LineMetrics> cm = new List<LineMetrics>();

        [ImportingConstructor()]
        public TextLayoutLineMetricsNode(DWriteFactory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInText.IsConnected == false)
            {
                this.metricsCount.SliceCount = 0;
                this.length.SliceCount = 0;
                this.trailingWhitespaceLength.SliceCount = 0;
                this.newlineLength.SliceCount = 0;
                this.height.SliceCount = 0;
                this.baseline.SliceCount = 0;
                this.isTrimmed.SliceCount = 0;
                return;
            }

            if (this.FInText.IsChanged)
            {
                this.metricsCount.SliceCount = SpreadMax;
               
                cm.Clear();
                for (int i = 0; i < SpreadMax;i++)
                {
                    TextLayout tl = this.FInText[i];
                    LineMetrics[] cms = tl.GetLineMetrics();
                    this.metricsCount[i] = cms.Length;
                    cm.AddRange(cms);
                }

                this.length.SliceCount = cm.Count;
                this.trailingWhitespaceLength.SliceCount = cm.Count;
                this.newlineLength.SliceCount = cm.Count;
                this.height.SliceCount = cm.Count;
                this.baseline.SliceCount = cm.Count;
                this.isTrimmed.SliceCount = cm.Count;

                for (int i = 0; i < cm.Count; i++)
                {
                    LineMetrics c = cm[i];
                    this.length[i] = c.Length;
                    this.trailingWhitespaceLength[i] = c.TrailingWhitespaceLength;
                    this.newlineLength[i] = c.NewlineLength;
                    this.height[i] = c.Height;
                    this.baseline[i] = c.Baseline;
                    this.isTrimmed[i] = c.IsTrimmed;
                }
            }
        }
    }
}
