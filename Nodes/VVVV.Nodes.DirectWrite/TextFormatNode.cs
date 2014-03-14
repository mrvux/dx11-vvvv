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
    [PluginInfo(Name="TextFormat", Category="DirectWrite")]
    public class TextFormatNode : IPluginEvaluate ,IDisposable
    {
        [Input("Font", EnumName = "SystemFonts")]
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
                || this.FStretch.IsChanged || this.FStyle.IsChanged || this.FWordWrap.IsChanged)
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    if (this.FOutput[i] != null) { this.FOutput[i].Dispose(); }
                }

                this.FOutput.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FOutput[i] = new TextFormat(this.dwFactory,this.FFontInput[i].Name, this.FWeight[i], this.FStyle[i], this.FStretch[i], FSize[i], "");
                    this.FOutput[i].WordWrapping = this.FWordWrap[i];
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
