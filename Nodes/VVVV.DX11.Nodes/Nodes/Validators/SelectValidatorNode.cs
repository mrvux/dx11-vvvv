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
    [PluginInfo(Name = "Select", Category = "DX11.Validator", Version = "", Author = "vux", Tags = "layer")]
    public class SelectObjectNode : IPluginEvaluate
    {
        [Input("Enable Slice", DefaultValue = 1)]
        protected ISpread<bool> FSliceEnabled;

        [Input("Enabled", IsSingle=true)]
        protected ISpread<bool> FInEnabled;

        [Output("Output",IsSingle=true)]
        protected ISpread<DX11SelectValidator> FOut;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOut[0] == null) { this.FOut[0] = new DX11SelectValidator(); }

            this.FOut[0].Enabled = this.FInEnabled[0];
            this.FOut[0].Selection = this.FSliceEnabled;


            this.FOut[0].Reset();

        }
    }
}
