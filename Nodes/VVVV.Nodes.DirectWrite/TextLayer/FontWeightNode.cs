using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "FontWeight", Category = "DirectWrite", Version = "Styles", Tags = "layout,text", Author = "vux")]
    public class FontWeightNode : TextStyleBaseNode
    {
        [Input("Weight")]
        protected IDiffSpread<FontWeight> style;

        private class FontWeighter : TextStyleBase
        {
            public FontWeight Style;

            protected override void DoApply(TextLayout layout, TextRange range)
            {
                layout.SetFontWeight(this.Style, range);
            }
        }

        protected override TextStyleBaseNode.TextStyleBase CreateStyle(int slice)
        {
            return new FontWeighter()
            {
                Style = style[slice]
            };
        }
    }
}
