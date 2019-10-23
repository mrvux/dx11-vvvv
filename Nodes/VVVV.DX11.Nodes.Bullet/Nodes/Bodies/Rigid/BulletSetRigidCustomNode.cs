using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;
using VVVV.Bullet.Core;

namespace VVVV.Bullet.Nodes.Bodies.Interactions.Rigid
{
    [PluginInfo(Name = "SetCustom", Category = "Bullet", Version = "Rigid", Author = "u7angel",
        Help = "Updates rigid body custom string", AutoEvaluate = true)]
    public class BulletSetCustomNode : IPluginEvaluate
    {
        [Input("Bodies", Order = 0)]
        protected ISpread<RigidBody> FInput;

        [Input("Custom String")]
        protected ISpread<string> FString;

        [Input("Set", IsBang =true)]
        protected IDiffSpread<bool> FSet;

        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                RigidBody rb = this.FInput[i];

                if (rb != null && FSet[i])
                {
                    BodyCustomData bd = (BodyCustomData)rb.UserObject;
                    bd.Custom = FString[i];
                }
            }
        }
    }
}
