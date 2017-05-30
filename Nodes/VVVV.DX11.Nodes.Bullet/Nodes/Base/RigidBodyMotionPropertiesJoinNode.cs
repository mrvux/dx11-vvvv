using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;
using VVVV.Internals.Bullet;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Bullet.DataTypes;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "MotionProperties", Category = "Bullet", Version = "Join",
        Help = "Builds bullet rigid body motion properties", Author = "vux")]
    public unsafe class BulletRigidBodyMotionPropertiesJoinNode : IPluginEvaluate
    {
        [Input("Linear Velocity")]
        protected ISpread<SlimDX.Vector3> linearVelocity;

        [Input("Angular Velocity")]
        protected ISpread<SlimDX.Vector3> angularVelocity;

        [Input("Allow Sleep")]
        protected ISpread<bool> allowSleep;

        [Output("Output")]
        protected ISpread<RigidBodyMotionProperties> output;

        public void Evaluate(int SpreadMax)
        {
            this.output.SliceCount = SpreadMax;

            fixed (RigidBodyMotionProperties* posePtr = &this.output.Stream.Buffer[0])
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    SlimDX.Vector3 lv = linearVelocity[i];
                    SlimDX.Vector3 av = angularVelocity[i];
                    posePtr[i].LinearVelocity = *(BulletSharp.Vector3*)&lv;
                    posePtr[i].AngularVelocity = *(BulletSharp.Vector3*)&av;
                    posePtr[i].AllowSleep = allowSleep[i];
                }
            }
            this.output.Flush(true);
        }
    }
}
