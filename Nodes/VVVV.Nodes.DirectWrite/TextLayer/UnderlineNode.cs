using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "UnderLine", Category = "DirectWrite", Tags = "layout,text")]
    public class UnderLine : BaseTextLayoutRangeFuncNode
    {
        protected override void Apply(TextLayout layout, bool enable, int slice)
        {
            var res = layout.SetUnderline(enable, new TextRange(ffrom[slice], fto[slice]));
        }
    }
}
