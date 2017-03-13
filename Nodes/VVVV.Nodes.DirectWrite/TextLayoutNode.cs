using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using DWriteFactory = SlimDX.DirectWrite.Factory;

namespace VVVV.DX11.Nodes.Nodes.Text
{
    [PluginInfo(Name = "TextLayout", Category = "DirectWrite", Author = "vux")]
    public class TextLayoutNode : IPluginEvaluate, IDisposable
    {
        [Input("Text")]
        protected IDiffSpread<string> FText;

        [Input("Format", CheckIfChanged=true)]
        protected Pin<TextFormat> FFormat;

        [Input("Text Alignment")]
        protected IDiffSpread<SharpDX.DirectWrite.TextAlignment> FTextAlign;

        [Input("Paragraph Alignment")]
        protected IDiffSpread<ParagraphAlignment> FParaAlign;

        [Input("Maximum Width", DefaultValue=100, MinValue =0.0)]
        protected IDiffSpread<float> FMaxWidth;

        [Input("Maximum Height", DefaultValue = 50, MinValue = 0.0)]
        protected IDiffSpread<float> FMaxHeight;

        [Output("Output")]
        protected ISpread<TextLayout> FOutput;

        private DWriteFactory dwFactory;

        [ImportingConstructor()]
        public TextLayoutNode(DWriteFactory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        public void Evaluate(int SpreadMax)
        {
            if (!this.FFormat.PluginIO.IsConnected)
            {
                this.FOutput.SliceCount = 0;
                return;
            }

            if (this.FFormat.IsChanged || this.FMaxHeight.IsChanged || this.FMaxWidth.IsChanged 
                || this.FText.IsChanged || this.FTextAlign.IsChanged || this.FParaAlign.IsChanged)
            {
                //first dispose old outputs
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    if (this.FOutput[i] != null) { this.FOutput[i].Dispose(); }
                }
                
                //then set new slicecount
                this.FOutput.SliceCount = SpreadMax;
                
                //then create new outputs
                for (int i = 0; i < SpreadMax; i++)
                {
                    float maxw = this.FMaxWidth[i] > 0.0f ? this.FMaxWidth[i] : 0.0f;
                    float maxh = this.FMaxHeight[i] > 0.0f ? this.FMaxHeight[i] : 0.0f;
                    var tl = new TextLayout(this.dwFactory, this.FText[i], this.FFormat[i], maxw, maxh);
                    var align = (int)this.FTextAlign[i];
                    tl.TextAlignment = (TextAlignment)align;
                    tl.ParagraphAlignment = this.FParaAlign[i];
                    this.FOutput[i] = tl;
                }
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                if (this.FOutput[i] != null) { this.FOutput[i].Dispose(); }
            }
        }
    }
}
