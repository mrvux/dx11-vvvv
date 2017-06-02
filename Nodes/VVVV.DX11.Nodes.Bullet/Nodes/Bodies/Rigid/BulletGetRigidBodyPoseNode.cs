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
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="GetBodyPose",Category="Bullet",
		Help = "Retrieves pose for a rigid body", Author = "vux")]
	public unsafe class BulletGetBodyPoseNode : IPluginEvaluate
	{
		[Input("Bodies")]
        protected Pin<RigidBody> bodies;

		[Output("Position")]
        protected ISpread<SlimDX.Vector3> position;

        [Output("Orientation")]
        protected ISpread<SlimDX.Quaternion> orientation;

        public void Evaluate(int SpreadMax)
		{
			if (this.bodies.PluginIO.IsConnected)
			{
                this.position.SliceCount = bodies.SliceCount;
                this.orientation.SliceCount = bodies.SliceCount;

                var pos = this.position.Stream.Buffer;
                var rot = this.orientation.Stream.Buffer;

				for (int i = 0; i < SpreadMax; i++)
				{
					RigidBody body = this.bodies[i];
                    pos[i] = new SlimDX.Vector3(body.MotionState.WorldTransform.M41,body.MotionState.WorldTransform.M42, body.MotionState.WorldTransform.M43);

                    Quaternion r= body.Orientation;
                    rot[i] = new SlimDX.Quaternion(r.X, r.Y, r.Z, r.W);

                }
                this.position.Flush(true);
                this.orientation.Flush(true);
            }
			else
			{
				this.position.SliceCount = 0;
                this.orientation.SliceCount = 0;
            }

		}
	}
}
