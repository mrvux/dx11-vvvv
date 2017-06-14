using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "SetPose", Category = "Bullet",Version="Rigid", Author = "vux",
		Help = "Updates rigid body pose", AutoEvaluate = true)]
	public class BulletUpdateRigidBodyNode : IPluginEvaluate
	{
		[Input("Bodies", Order = 0)]
        protected ISpread<RigidBody> FInput;

		[Input("Pose")]
        protected Pin<RigidBodyPose> FPose;

		[Input("Apply", IsBang = true)]
        protected IDiffSpread<bool> FSetPosition;


		public void Evaluate(int SpreadMax)
		{

            for (int i = 0; i < SpreadMax; i++)
            {
                RigidBody rb = this.FInput[i];

                if (rb != null && this.FSetPosition[i])
                {
                    RigidBodyPose pose = FPose.IsConnected ? FPose[i] : RigidBodyPose.Default;

                    Matrix transform = (Matrix)pose;
                    rb.WorldTransform = transform;
                    rb.MotionState.WorldTransform = transform;
                }

            }
		}
	}
}
