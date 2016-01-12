using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "UnderLine", Category = "DirectWrite",Version="Styles", Tags = "layout,text")]
    public class UnderLineNode : TextStyleBaseNode
    {
        private class Underliner : TextStyleBase
        {
            protected override void DoApply(TextLayout layout, TextRange range)
            {
                layout.SetUnderline(true, range);
            }
        }

        protected override TextStyleBaseNode.TextStyleBase CreateStyle(int slice)
        {
            return new Underliner();
        }
    }
}
