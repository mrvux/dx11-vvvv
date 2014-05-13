using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "FontSize", Category = "DirectWrite", Tags = "layout,text")]
    public class FontSizeNode : BaseTextLayoutRangeFuncNode
    {
        [Input("Size", DefaultValue = 32)]
        IDiffSpread<float> size;

        protected override bool IsChanged()
        {
            return base.IsChanged() || size.IsChanged;
        }

        protected override void Apply(TextLayout layout, bool enable, int slice)
        {
            var res = layout.SetFontSize(size[slice], new TextRange(ffrom[slice], fto[slice]));
        }
    }
}
