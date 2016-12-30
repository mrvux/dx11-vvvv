using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using VVVV.DX11.Validators;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "GetSpread", Category = "DX11.Validator", Version = "", Author = "vux", Tags = "layer")]
    public class LayerGetSpreadNode : IPluginEvaluate
    {
        [Input("Enable Slice", DefaultValue = 1)]
        protected ISpread<bool> FInEnabled;

        [Input("Slice Start", IsSingle = true)]
        protected ISpread<int> FinMin;

        [Input("Slice End", IsSingle = true, DefaultValue = 0)]
        protected ISpread<int> FInMax;

        [Output("Output", IsSingle = true)]
        protected ISpread<DX11SliceRangeValidator> FOut;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOut[0] == null) { this.FOut[0] = new DX11SliceRangeValidator(); }

            this.FOut[0].Enabled = this.FInEnabled[0];
            this.FOut[0].MinIndex = this.FinMin[0];
            this.FOut[0].MaxIndex = this.FInMax[0];


            this.FOut[0].Reset();

        }
    }
}
