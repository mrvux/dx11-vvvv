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
	[PluginInfo(Name="GetBoundingSphere",Category="Bullet",
		Help = "Retrieves details for a rigid body", Author = "vux")]
	public class BulletGetRigidBodySphereNode : IPluginEvaluate
	{
		[Input("Bodies")]
        protected Pin<RigidBody> FBodies;

		[Output("Position")]
        protected ISpread<Vector3D> FPosition;

		[Output("Radius")]
        protected ISpread<float> FRadius;

		[Output("Id")]
        protected ISpread<int> FId;

		public void Evaluate(int SpreadMax)
		{
			if (this.FBodies.IsConnected)
			{
				this.FId.SliceCount = this.FBodies.SliceCount;
				this.FPosition.SliceCount = this.FBodies.SliceCount;
                this.FRadius.SliceCount = this.FBodies.SliceCount;

                for (int i = 0; i < SpreadMax; i++)
                {
                    RigidBody body = this.FBodies[i];

                    BodyCustomData bd = (BodyCustomData)body.UserObject;

                    this.FId[i] = bd.Id;
                    CollisionShape shape = body.CollisionShape;
                    Vector3 center;
                    float radius;
                    shape.GetBoundingSphere(out center, out radius);
                    this.FPosition[i] = center.ToVVVVector();
                    this.FRadius[i] = radius;
                }

			}
			else
			{
				this.FId.SliceCount = 0;
				this.FPosition.SliceCount = 0;
                this.FRadius.SliceCount = 0;
			}
		}
	}
}
