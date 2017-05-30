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
    [PluginInfo(Name = "Pose", Category = "Bullet", Version ="Join",
        Help = "Builds a rigid body pose (position + orientation)", Author = "vux")]
    public unsafe class BulletRigidBodyPoseNode : IPluginEvaluate
    {
        [Input("Position")]
        protected ISpread<SlimDX.Vector3> position;

        [Input("Orientation", DefaultValues = new double[] { 0.0, 0.0, 0.0, 1.0 })]
        protected ISpread<SlimDX.Quaternion> orientation;

        [Output("Output")]
        protected ISpread<RigidBodyPose> output;

        public void Evaluate(int SpreadMax)
        {
            this.output.SliceCount = SpreadMax;
            
            fixed (RigidBodyPose* posePtr = &this.output.Stream.Buffer[0])
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    SlimDX.Vector3 p = position[i];
                    SlimDX.Quaternion q = orientation[i];
                    posePtr[i].Position = *(BulletSharp.Vector3*)&p;
                    posePtr[i].Orientation = *(BulletSharp.Quaternion*)&q;
                }
            }
            this.output.Flush(true);
        }
    }
}
