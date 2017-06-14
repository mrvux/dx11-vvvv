using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "SetVelocity", Category = "Bullet", Version = "Rigid", Author = "vux",
        Help = "Updates rigid body properties", AutoEvaluate = true)]
    public class BulletSetVelocityNode : IPluginEvaluate
    {
        [Input("Bodies", Order = 0)]
        protected ISpread<RigidBody> FInput;

        [Input("Linear Velocity")]
        protected ISpread<Vector3D> FLinVel;

        [Input("Set Linear Velocity", IsBang = true)]
        protected IDiffSpread<bool> FSetLinVel;

        [Input("Angular Velocity")]
        protected ISpread<Vector3D> FAngVel;

        [Input("Set Angular Velocity", IsBang = true)]
        protected IDiffSpread<bool> FSetAngVel;

        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                RigidBody rb = this.FInput[i];

                if (rb != null)
                {
                    if (this.FSetLinVel[i])
                    {
                        rb.LinearVelocity = this.FLinVel[i].ToBulletVector();
                    }
                    if (this.FSetAngVel[i])
                    {
                        rb.AngularVelocity = this.FAngVel[i].ToBulletVector();
                    }
                }
            }
        }
    }
}
