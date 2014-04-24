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
    [PluginInfo(Name = "MeasureText", Category = "String", Version = "", Author = "vux")]
    public class MeasureTextNode : IPluginEvaluate
    {
        [Input("Text")]
        protected IDiffSpread<string> FInText;

        [Input("Format")]
        protected IDiffSpread<TextFormat> FFormat;

        [Output("Left")]
        protected ISpread<float> FLeft;

        [Output("Width")]
        protected ISpread<float> FWidth;

        [Output("Layout Width")]
        protected ISpread<float> FLayoutWidth;

        [Output("Position")]
        protected ISpread<float> FPosition;

        private DWriteFactory dwFactory;

        [ImportingConstructor()]
        public MeasureTextNode(DWriteFactory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInText.IsChanged || this.FFormat.IsChanged)
            {
                TextFormat format = this.FFormat[0];

                SlimDX.DirectWrite.TextLayout layout = new TextLayout(this.dwFactory, this.FInText[0], format,50000,50000);
                float lastwidth = 0.0f;
                string txt = this.FInText[0];

                this.FPosition.SliceCount = txt.Length;
                this.FWidth[0] = layout.Metrics.WidthIncludingTrailingWhitespace;
                this.FLayoutWidth[0] = layout.Metrics.LayoutWidth;
                this.FLeft[0] = layout.Metrics.Left;

                this.FPosition[0] = 0;
                string t = txt.Substring(0, 1);
                var ly = new TextLayout(this.dwFactory, t, format, 50000, 50000);
                lastwidth = ly.Metrics.WidthIncludingTrailingWhitespace;

                for (int i = 1; i < txt.Length; i++)
                {
                    t = txt.Substring(0, i + 1);
                    ly = new TextLayout(this.dwFactory, t, format, 50000, 50000);
                    float s = ly.Metrics.WidthIncludingTrailingWhitespace;
                    this.FPosition[i] = lastwidth;
                    lastwidth = s;
                }

            }
        }
    }
}
