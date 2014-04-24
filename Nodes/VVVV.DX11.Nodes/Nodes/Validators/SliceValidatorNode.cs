using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using VVVV.PluginInterfaces.V2;

using FeralTic.Resources.Geometry;

using VVVV.DX11.Validators;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "GetSlice", Category = "DX11.Validator", Version = "", Author = "vux", Tags = "layer")]
    public class LayerGetSliceNode : IPluginEvaluate
    {
        [Input("Enable Slice", DefaultValue = 1)]
        protected ISpread<bool> FInEnabled;

        [Input("Index", IsSingle = true)]
        protected ISpread<int> FInIndex;

        [Output("Output", IsSingle = true)]
        protected ISpread<DX11SliceValidator> FOut;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOut[0] == null) { this.FOut[0] = new DX11SliceValidator(); }

            this.FOut[0].Enabled = this.FInEnabled[0];
            this.FOut[0].Index = this.FInIndex[0];


            this.FOut[0].Reset();

        }
    }


}
