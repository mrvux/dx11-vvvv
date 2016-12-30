using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "Spacing", Category = "DirectWrite",Version="Styles", Tags = "layout,text")]
    public class FontSpacingNode : TextStyleBaseNode
    {
        [Input("Leading Spacing", DefaultValue = 0)]
        protected IDiffSpread<float> leading;

        [Input("Trailing Spacing", DefaultValue = 0)]
        protected IDiffSpread<float> trailing;

        [Input("Minimum Advance Width", DefaultValue = 0)]
        protected IDiffSpread<float> minadv;

        private class FontSpacing : TextStyleBase
        {
            public float lead;
            public float trail;
            public float minw;


            protected override void DoApply(TextLayout layout, TextRange range)
            {
                SharpDX.DirectWrite.TextLayout1 tl = new SharpDX.DirectWrite.TextLayout1(layout.ComPointer);

                tl.SetCharacterSpacing(this.lead, this.trail, this.minw, new SharpDX.DirectWrite.TextRange(range.StartPosition, range.Length));
            }
        }

        protected override TextStyleBaseNode.TextStyleBase CreateStyle(int slice)
        {
            return new FontSpacing()
            {
                lead = leading[slice],
                trail = trailing[slice],
                minw = minadv[slice]
            };
        }
    }
}

