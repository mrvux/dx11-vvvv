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
	[PluginInfo(Name="GetRigidBodyVelocity",Category="Bullet",
		Help = "Retrieves details for a rigid body", Author = "vux")]
	public class BulletGetRigidBodyVelocityNode : IPluginEvaluate
	{
		[Input("Bodies")]
        protected Pin<RigidBody> FBodies;

		[Output("Linear Velocity")]
        protected ISpread<Vector3D> FLinVel;

		[Output("Angular Velocity")]
        protected ISpread<Vector3D> FAngVel;

		[Import()]
        protected ILogger FLogger;

		public void Evaluate(int SpreadMax)
		{
			if (this.FBodies.PluginIO.IsConnected)
			{
				this.FLinVel.SliceCount = this.FBodies.SliceCount;
				this.FAngVel.SliceCount = this.FBodies.SliceCount;

				for (int i = 0; i < SpreadMax; i++)
				{
					RigidBody body = this.FBodies[i];
					this.FLinVel[i] = body.LinearVelocity.ToVVVVector();
					this.FAngVel[i] = body.AngularVelocity.ToVVVVector();
				}
			}
			else
			{
				this.FLinVel.SliceCount = 0;
				this.FAngVel.SliceCount = 0;
			}

		}
	}
}
