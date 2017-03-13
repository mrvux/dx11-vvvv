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
        [Input("Text", AutoValidate = false)]
        protected ISpread<string> FText;

        [Input("Format", AutoValidate=false)]
        protected Pin<TextFormat> FFormat;

        [Input("Text Alignment", AutoValidate = false)]
        protected ISpread<SharpDX.DirectWrite.TextAlignment> FTextAlign;

        [Input("Paragraph Alignment", AutoValidate = false)]
        protected ISpread<ParagraphAlignment> FParaAlign;

        [Input("Maximum Width", DefaultValue = 100, AutoValidate = false, MinValue = 0.0)]
        protected ISpread<float> FMaxWidth;

        [Input("Maximum Height", DefaultValue = 50, AutoValidate = false, MinValue = 0.0)]
        protected ISpread<float> FMaxHeight;

        [Input("Styles", AutoValidate=false)]
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
                this.FText.Sync();
                this.FTextAlign.Sync();
                this.FParaAlign.Sync();
                this.FMaxHeight.Sync();
                this.FMaxWidth.Sync();
                this.FFormat.Sync();
                this.textStyles.Sync();

                //first dispose old outputs
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    if (this.FOutput[i] != null) { this.FOutput[i].Dispose(); }
                }

                var spMax = SpreadUtils.SpreadMax(this.FText, this.FTextAlign, this.FParaAlign, this.FMaxHeight, this.FMaxWidth, this.FFormat, this.textStyles);

                //then set new slicecount
                this.FOutput.SliceCount = spMax;
                
                //then create new outputs
                for (int i = 0; i < spMax; i++)
                {
                    float maxw = this.FMaxWidth[i] > 0.0f ? this.FMaxWidth[i] : 0.0f;
                    float maxh = this.FMaxHeight[i] > 0.0f ? this.FMaxHeight[i] : 0.0f;
                    var tl = new TextLayout(this.dwFactory, this.FText[i], this.FFormat[i], maxw, maxh);
                    var align = (int)this.FTextAlign[i];
                    tl.TextAlignment = (TextAlignment)align;
                    tl.ParagraphAlignment = this.FParaAlign[i];
                    var styles = textStyles[i];
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
