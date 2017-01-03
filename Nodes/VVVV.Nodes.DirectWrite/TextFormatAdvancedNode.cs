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
    [PluginInfo(Name = "TextFormat", Category = "DirectWrite",Version ="Advanced", Author = "vux", Help ="TextFormat node with alignment options")]
    public class TextFormatAdvNode : IPluginEvaluate ,IDisposable
    {
        [Input("Font", EnumName = "DirectWrite_Font_Families")]
        protected IDiffSpread<EnumEntry> FFontInput;

        [Input("Font Size", DefaultValue = 12)]
        protected IDiffSpread<int> FSize;

        [Input("Font Weight", DefaultEnumEntry="Normal")]
        protected IDiffSpread<FontWeight> FWeight;

        [Input("Font Style", DefaultEnumEntry = "Normal")]
        protected IDiffSpread<FontStyle> FStyle;

        [Input("Font Stretch", DefaultEnumEntry = "Normal")]
        protected IDiffSpread<FontStretch> FStretch;

        [Input("Word Wrapping")]
        protected IDiffSpread<WordWrapping> FWordWrap;

        [Input("Line Spacing Method")]
        protected IDiffSpread<LineSpacingMethod> FMethod;

        [Input("Line Spacing", DefaultValue = 12)]
        protected IDiffSpread<float> FLineSpacing;

        [Input("Baseline", DefaultValue = 12)]
        protected IDiffSpread<int> FBaseLine;

        [Input("Text Alignment")]
        protected IDiffSpread<SharpDX.DirectWrite.TextAlignment> FTextAlign;

        [Input("Paragraph Alignment")]
        protected IDiffSpread<ParagraphAlignment> FParaAlign;

        [Output("Output")]
        protected ISpread<TextFormat> FOutput;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private DWriteFactory dwFactory;

        [ImportingConstructor()]
        public TextFormatAdvNode(DWriteFactory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FSize.IsChanged || this.FFontInput.IsChanged || this.FWeight.IsChanged 
                || this.FStretch.IsChanged || this.FStyle.IsChanged || this.FWordWrap.IsChanged
                || this.FLineSpacing.IsChanged || this.FMethod.IsChanged || this.FBaseLine.IsChanged
                || this.FTextAlign.IsChanged || this.FParaAlign.IsChanged
                )
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    if (this.FOutput[i] != null) { this.FOutput[i].Dispose(); }
                }

                this.FOutput.SliceCount = SpreadMax;
                this.FValid.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    try
                    {
                        TextFormat format = new TextFormat(this.dwFactory, this.FFontInput[i].Name, this.FWeight[i], this.FStyle[i], this.FStretch[i], FSize[i], "");
                        format.WordWrapping = this.FWordWrap[i];
                        format.SetLineSpacing(this.FMethod[i], this.FLineSpacing[i], this.FBaseLine[i]);
                        var align = (int)this.FTextAlign[i];
                        format.TextAlignment = (TextAlignment)align;
                        format.ParagraphAlignment = this.FParaAlign[i];
                        this.FOutput[i] = format;
                        this.FValid[i] = true;
                    }
                    catch
                    {
                        //Set default format
                        TextFormat format = new TextFormat(this.dwFactory, "Arial", FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, 16, "");
                        this.FOutput[i] = format;
                        this.FValid[i] = false;
                    }
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
