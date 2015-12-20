using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "StrikeTrough", Category = "DirectWrite",Version="Styles", Tags = "layout,text")]
    public class StrikeTroughNodeLine : TextStyleBaseNode
    {
        private class StrikeTrough : TextStyleBase
        {
            protected override void DoApply(TextLayout layout, TextRange range)
            {
                layout.SetStrikethrough(true, range);
            }
        }

        protected override TextStyleBaseNode.TextStyleBase CreateStyle(int slice)
        {
            return new StrikeTrough();
        }
    }
}
