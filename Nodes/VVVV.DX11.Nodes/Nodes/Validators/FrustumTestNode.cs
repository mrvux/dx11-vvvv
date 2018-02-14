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
    [PluginInfo(Name = "FrustumTest", Category = "DX11.Validator", Version = "", Author = "vux",Tags="layer")]
    public class FrustumTestNode : IPluginEvaluate
    {
        [Input("Enabled", IsSingle=true)]
        protected ISpread<bool> FInEnabled;

        [Output("Output",IsSingle=true)]
        protected ISpread<DX11FrustumValidator> FOut;

        [Output("Passed",IsSingle=true)]
        protected ISpread<int> FOutPass;

        [Output("Failed",IsSingle=true)]
        protected ISpread<int> FOutFail;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOut[0] == null) { this.FOut[0] = new DX11FrustumValidator();}

            this.FOut[0].Enabled = this.FInEnabled[0];

            this.FOutPass[0] = this.FOut[0].Passed;
            this.FOutFail[0] = this.FOut[0].Failed;

            this.FOut[0].Reset();

        }
    }
}
