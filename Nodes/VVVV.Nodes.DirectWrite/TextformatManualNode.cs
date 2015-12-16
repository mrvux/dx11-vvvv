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
    [PluginInfo(Name="TextFormat", Category="DirectWrite", Version="Advanced")]
    public class TextFormatAdvancedNode : IPluginEvaluate ,IDisposable
    {
        [Input("Font")]
        protected IDiffSpread<string> FFontInput;

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

        [Output("Font Exists")]
        protected ISpread<bool> FOutNameExists;

        [Output("Is Valid")]
        protected ISpread<bool> FOutValid;

        private DWriteFactory dwFactory;

        [ImportingConstructor()]
        public TextFormatAdvancedNode(DWriteFactory dwFactory)
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
                this.FOutNameExists.SliceCount = SpreadMax;
                this.FOutValid.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    bool exists;
                    var ff = dwFactory.GetSystemFontCollection(false).FindFamilyName(this.FFontInput[i], out exists);
                    this.FOutNameExists[i] = exists;
                    this.FOutput[i] = new TextFormat(this.dwFactory,this.FFontInput[i], this.FWeight[i], this.FStyle[i], this.FStretch[i], FSize[i], "");
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
