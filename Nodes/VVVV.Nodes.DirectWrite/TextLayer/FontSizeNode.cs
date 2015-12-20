using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "FontSize", Category = "DirectWrite",Version="Styles", Tags = "layout,text")]
    public class FontSizeNode : TextStyleBaseNode
    {
        [Input("Size", DefaultValue = 32)]
        protected IDiffSpread<float> size;

        private class FontSize : TextStyleBase
        {
            public float Size;

            protected override void DoApply(TextLayout layout, TextRange range)
            {
                layout.SetFontSize(this.Size, range);
            }
        }

        protected override TextStyleBaseNode.TextStyleBase CreateStyle(int slice)
        {
            return new FontSize()
            {
                Size = size[slice]
            };
        }
    }
}

