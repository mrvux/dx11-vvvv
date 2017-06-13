using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Bullet.Core;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "SetHingeMotor", Author = "vux", Category = "Bullet", Version = "Constraint.Single", AutoEvaluate = true)]
    public class SetHingeMotor : IPluginEvaluate
    {
        [Input("Constraint")]
        protected ISpread<HingeConstraint> FConstraint;

        [Input("Target Velocity", DefaultValue =-1.0f)]
        protected ISpread<float> FTargetVelocity;

        [Input("Max Motor Impulse", DefaultValue = 1.0f)]
        protected ISpread<float> FMaxImpulse;

        [Input("Enabled")]
        protected ISpread<bool> FEnabled;

        [Input("Apply", IsBang=true)]
        protected ISpread<bool> FApply;

        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FConstraint[i] != null && FApply[i])
                {
                    HingeConstraint cst = this.FConstraint[i];
                    cst.EnableAngularMotor(this.FEnabled[i], this.FTargetVelocity[i], this.FMaxImpulse[i]);
                }
            }
        }
    }
}
