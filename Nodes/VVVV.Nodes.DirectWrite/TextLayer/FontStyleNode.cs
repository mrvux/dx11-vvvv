using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "FontStyle", Category = "DirectWrite", Version = "Styles", Tags = "layout,text", Author = "vux")]
    public class FontStyleNode : TextStyleBaseNode
    {
        [Input("Style")]
        protected IDiffSpread<FontStyle> style;

        private class FontStyler : TextStyleBase
        {
            public FontStyle Style;

            protected override void DoApply(TextLayout layout, TextRange range)
            {
                layout.SetFontStyle(this.Style, range);
            }
        }

        protected override TextStyleBaseNode.TextStyleBase CreateStyle(int slice)
        {
            return new FontStyler()
            {
                Style = style[slice]
            };
        }
    }
}
