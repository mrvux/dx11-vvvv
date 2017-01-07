using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX;
using FeralTic.Core.Maths;
using SlimDX.DirectWrite;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Cons", Category = "TextObject", Version = "", Tags = "", Author = "vux")]
    public class ConsTextObjects : ConsNonNilNode<TextObject>
    {
    }

    [PluginInfo(Name = "TextObject", Category = "Text", Version = "", Author = "vux")]
    public class TextObjectNode : IPluginEvaluate
    {
        [Input("Text Format")]
        protected ISpread<TextFormat> textFormat;

        [Input("Text")]
        protected ISpread<string> input;

        [Input("Transform In")]
        protected ISpread<Matrix> transformIn;

        [Input("Color")]
        protected ISpread<Color4> color;

        [Output("Output")]
        protected ISpread<TextObject> output;

        public void Evaluate(int SpreadMax)
        {
            this.output.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                this.output[i] = new TextObject()
                {
                    Color = this.color[i],
                    Matrix = this.transformIn[i],
                    Text = this.input[i],
                    TextFormat = this.textFormat[i]
                };                  
            }
        }
    }
}
