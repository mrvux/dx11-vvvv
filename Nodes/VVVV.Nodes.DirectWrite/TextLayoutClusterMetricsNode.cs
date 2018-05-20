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
    [PluginInfo(Name = "ClusterMetrics", Category = "DirectWrite", Version = "TextLayout", Author = "vux")]
    public class TextLayoutClusterMetricsNode : IPluginEvaluate
    {
        [Input("Text Layout", CheckIfChanged = true)]
        protected Pin<TextLayout> FInText;

        [Output("Cluster Metrics Count")]
        protected ISpread<int> metricsCount;

        [Output("Can Wrap Line After")]
        protected ISpread<bool> FCanWrapLineAfter;

        [Output("Is New Line")]
        protected ISpread<bool> FNewLine;

        [Output("Is Right To Left")]
        protected ISpread<bool> FIsRightToLeft;

        [Output("Is Soft Hyphen")]
        protected ISpread<bool> FIsSoftHyphent;

        [Output("Is Whitespace")]
        protected ISpread<bool> FIsWhitespace;

        [Output("Length")]
        protected ISpread<int> FLength;

        [Output("Width")]
        protected ISpread<float> FWidth;

        private DWriteFactory dwFactory;
        private List<ClusterMetrics> cm = new List<ClusterMetrics>();

        [ImportingConstructor()]
        public TextLayoutClusterMetricsNode(DWriteFactory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInText.IsConnected == false)
            {
                this.metricsCount.SliceCount = 0;
                this.FCanWrapLineAfter.SliceCount = 0;
                this.FIsRightToLeft.SliceCount = 0;
                this.FIsSoftHyphent.SliceCount = 0;
                this.FIsWhitespace.SliceCount = 0;
                this.FLength.SliceCount = 0;
                this.FNewLine.SliceCount = 0;
                this.FWidth.SliceCount = 0;
                return;
            }

            if (this.FInText.IsChanged)
            {
                this.metricsCount.SliceCount = SpreadMax;
               
                cm.Clear();
                for (int i = 0; i < SpreadMax;i++)
                {
                    TextLayout tl = this.FInText[i];
                    ClusterMetrics[] cms = tl.GetClusterMetrics();
                    this.metricsCount[i] = cms.Length;
                    cm.AddRange(cms);
                }

                this.FCanWrapLineAfter.SliceCount = cm.Count;
                this.FIsRightToLeft.SliceCount = cm.Count;
                this.FIsSoftHyphent.SliceCount = cm.Count;
                this.FIsWhitespace.SliceCount = cm.Count;
                this.FLength.SliceCount = cm.Count;
                this.FNewLine.SliceCount = cm.Count;
                this.FWidth.SliceCount = cm.Count;

                for (int i = 0; i < cm.Count; i++)
                {
                    ClusterMetrics c = cm[i];
                    this.FCanWrapLineAfter[i] = c.CanWrapLineAfter;
                    this.FIsRightToLeft[i] = c.IsRightToLeft;
                    this.FIsSoftHyphent[i] = c.IsSoftHyphen;
                    this.FIsWhitespace[i] = c.IsWhitespace;
                    this.FLength[i] = c.Length;
                    this.FWidth[i] = c.Width;
                }
            }
        }
    }
}
