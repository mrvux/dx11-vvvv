using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "PairKerning", Category = "DirectWrite",Version="Styles", Tags = "layout,text")]
    public class FontPairKerningNode : TextStyleBaseNode
    {
        private class FontPairKerning : TextStyleBase
        {
            protected override void DoApply(TextLayout layout, TextRange range)
            {
                SharpDX.DirectWrite.TextLayout1 tl = new SharpDX.DirectWrite.TextLayout1(layout.ComPointer);
                tl.SetPairKerning(true, new SharpDX.DirectWrite.TextRange(range.StartPosition, range.Length));
            }
        }

        protected override TextStyleBaseNode.TextStyleBase CreateStyle(int slice)
        {
            return new FontPairKerning();
        }
    }
}

