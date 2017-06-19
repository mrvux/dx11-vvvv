using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "Activate", Category = "Bullet", Version = "Rigid", Author = "vux",
        Help = "Manually activates a  rigid body pose", AutoEvaluate = true)]
    public class BulletActivateRigidBodyNode : IPluginEvaluate
    {
        [Input("Bodies", Order = 0)]
        protected ISpread<RigidBody> FInput;

        [Input("Activate", IsBang = true)]
        protected IDiffSpread<bool> FActivate;

        public void Evaluate(int SpreadMax)
        {

            for (int i = 0; i < SpreadMax; i++)
            {
                RigidBody rb = this.FInput[i];

                if (rb != null && this.FActivate[i])
                {
                    rb.Activate();
                }
            }
        }
    }
}
