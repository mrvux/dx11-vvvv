using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "FontFamily", Category = "DirectWrite",Version="Styles", Tags = "layout,text", Author="vux")]
    public class FontFamilyNode : TextStyleBaseNode
    {
        [Input("Font", EnumName = "DirectWrite_Font_Families")]
        protected IDiffSpread<EnumEntry> FFontInput;

        private class FontFamilyStyle : TextStyleBase
        {
            public string Name;

            protected override void DoApply(TextLayout layout, TextRange range)
            {
                layout.SetFontFamilyName(Name, range);
            }
        }

        protected override TextStyleBaseNode.TextStyleBase CreateStyle(int slice)
        {
            return new FontFamilyStyle()
            {
                Name = FFontInput[slice]
            };
        }
    }
}


