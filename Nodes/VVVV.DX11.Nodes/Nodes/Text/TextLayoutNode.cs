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
    [PluginInfo(Name = "TextLayout", Category = "DirectWrite")]
    public class TextLayoutNode : IPluginEvaluate, IDisposable
    {
        [Input("Text")]
        protected IDiffSpread<string> FText;

        [Input("Format")]
        protected IDiffSpread<TextFormat> FFormat;

        [Input("Maximum Width", DefaultValue=100)]
        protected IDiffSpread<float> FMaxWidth;

        [Input("Maximum Height", DefaultValue = 50)]
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
            if (this.FFormat.IsChanged || this.FMaxHeight.IsChanged || this.FMaxWidth.IsChanged)
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    if (this.FOutput[i] != null) { this.FOutput[i].Dispose(); }
                }

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FOutput[i] = new TextLayout(this.dwFactory, this.FText[i], this.FFormat[i], this.FMaxWidth[i], this.FMaxHeight[i]);
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
