using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core.DirectWrite;
using VVVV.DX11.Nodes;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "Cons", Category = "DirectWrite",Version="Styles", Tags = "layout,text", Author="vux")]
    public class ConsFontStyles : ConsNonNilNode<ITextStyler>
    {
    }
}

