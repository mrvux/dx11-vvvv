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
    [PluginInfo(Name = "TextFormat", Category = "DirectWrite", Author = "vux")]
    public class TextFormatNode : IPluginEvaluate ,IDisposable
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

        [Output("Output")]
        protected ISpread<TextFormat> FOutput;

        private DWriteFactory dwFactory;

        [ImportingConstructor()]
        public TextFormatNode(DWriteFactory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FSize.IsChanged || this.FFontInput.IsChanged || this.FWeight.IsChanged 
                || this.FStretch.IsChanged || this.FStyle.IsChanged || this.FWordWrap.IsChanged
                || this.FLineSpacing.IsChanged || this.FMethod.IsChanged || this.FBaseLine.IsChanged)
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    if (this.FOutput[i] != null) { this.FOutput[i].Dispose(); }
                }

                this.FOutput.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    string familyName = this.FFontInput[i].Name;

                    var fc = this.dwFactory.GetSystemFontCollection(false);
                    bool exists;
                    int idx = fc.FindFamilyName(this.FFontInput[i].Name, out exists);
                    this.FOutput[i] = new TextFormat(this.dwFactory,this.FFontInput[i].Name, this.FWeight[i], this.FStyle[i], this.FStretch[i], FSize[i], "");
                    this.FOutput[i].WordWrapping = this.FWordWrap[i];
                    this.FOutput[i].SetLineSpacing(this.FMethod[i], this.FLineSpacing[i], this.FBaseLine[i]);
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
