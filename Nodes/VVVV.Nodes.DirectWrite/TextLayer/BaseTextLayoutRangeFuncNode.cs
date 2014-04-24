using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    public abstract class BaseTextLayoutRangeFuncNode : BaseTextLayoutFuncNode
    {
        [Input("From")]
        protected IDiffSpread<int> ffrom;

        [Input("Length")]
        protected IDiffSpread<int> fto;

        protected override bool IsChanged()
        {
            return ffrom.IsChanged || fto.IsChanged;
        }
    }
}
