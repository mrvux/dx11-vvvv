using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "Style", Category = "DirectWrite", Tags = "layout,text")]
    public class FontStyleNode : BaseTextLayoutRangeFuncNode
    {
        [Input("Style")]
        protected IDiffSpread<FontStyle> style;

        [Input("Weight")]
        protected IDiffSpread<FontWeight> w;

        [Input("Stretch")]
        protected IDiffSpread<FontStretch> s;

        protected override bool IsChanged()
        {
            return base.IsChanged() || style.IsChanged || w.IsChanged || s.IsChanged;
        }

        protected override void Apply(TextLayout layout, bool enable, int slice)
        {

            var res = layout.SetFontStyle(this.style[slice], new TextRange(ffrom[slice], fto[slice]));
            layout.SetFontWeight(this.w[slice], new TextRange(ffrom[slice], fto[slice]));
            layout.SetFontStretch(this.s[slice], new TextRange(ffrom[slice], fto[slice]));
        }
    }
}
