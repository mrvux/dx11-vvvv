using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "FontFamily", Category = "DirectWrite", Tags = "layout,text")]
    public class FontFamilyNode : BaseTextLayoutRangeFuncNode
    {
        [Input("Font", EnumName = "SystemFonts")]
        protected IDiffSpread<EnumEntry> FFontInput;

        protected override bool IsChanged()
        {
            return base.IsChanged() || FFontInput.IsChanged;
        }

        protected override void Apply(TextLayout layout, bool enable, int slice)
        {
            var res = layout.SetFontFamilyName(this.FFontInput[slice].Name, new TextRange(ffrom[slice], fto[slice]));
        }
    }
}
