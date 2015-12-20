using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.Core.DirectWrite;
using VVVV.Nodes.DirectWrite;
using VVVV.PluginInterfaces.V2;
using DWriteFactory = SlimDX.DirectWrite.Factory;

namespace VVVV.DX11.Nodes.Text
{
    [PluginInfo(Name = "TextLayout", Category = "DirectWrite", Version = "Advanced", Author = "vux")]
    public class TextLayoutAdvancedNode : IPluginEvaluate, IDisposable
    {
        [Input("Text")]
        protected IDiffSpread<string> FText;

        [Input("Format", CheckIfChanged=true)]
        protected Pin<TextFormat> FFormat;

        [Input("Text Alignment")]
        protected IDiffSpread<TextAlignment> FTextAlign;

        [Input("Paragraph Alignment")]
        protected IDiffSpread<ParagraphAlignment> FParaAlign;

        [Input("Maximum Width", DefaultValue=100)]
        protected IDiffSpread<float> FMaxWidth;

        [Input("Maximum Height", DefaultValue = 50)]
        protected IDiffSpread<float> FMaxHeight;

        [Input("Styles")]
        protected ISpread<ISpread<ITextStyler>> textStyles;

        [Input("Apply", IsBang=true)]
        protected ISpread<bool> apply;

        [Output("Output")]
        protected ISpread<TextLayout> FOutput;

        private DWriteFactory dwFactory;
        private bool first = true;

        [ImportingConstructor()]
        public TextLayoutAdvancedNode(DWriteFactory dwFactory)
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

            if (this.apply[0] || this.first)
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
                    var tl = new TextLayout(this.dwFactory, this.FText[i], this.FFormat[i], this.FMaxWidth[i], this.FMaxHeight[i]);
                    tl.TextAlignment = this.FTextAlign[i];
                    tl.ParagraphAlignment = this.FParaAlign[i];
                    var styles = textStyles[0];
                    for (int j = 0; j <styles.SliceCount; j++)
                    {
                        if (styles[j] != null)
                        {
                            styles[j].Apply(tl);
                        }
                    }
                    
                    this.FOutput[i] = tl;
                }
            }
            this.first = false;
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
