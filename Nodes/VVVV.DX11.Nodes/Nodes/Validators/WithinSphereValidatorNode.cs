using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.DX11.Validators;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "WithinSphere", Category = "DX11.Validator", Help = "Validates objects and only draw the ones that are within a bounding sphere", Author ="vux")]
    public class ValidatorWithinSphereNode : IPluginEvaluate
    {
        [Input("Center", DefaultValue = 0.0)]
        public ISpread<Vector3> FInput;

        [Input("Radius", DefaultValue = 0.5)]
        public ISpread<float> FRadius;

        [Input("Enabled", DefaultValue = 1)]
        protected ISpread<bool> FInEnabled;

        [Output("Output")]
        public ISpread<WithinSphereValidator> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutput[0] == null) { this.FOutput[0] = new WithinSphereValidator(); }
            this.FOutput[0].Enabled = this.FInEnabled[0];
            this.FOutput[0].BoundingSphere = new BoundingSphere(this.FInput[0], this.FRadius[0]);
            this.FOutput[0].Reset();
        }
    }
}
