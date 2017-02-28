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

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="BodyTransform",Category="Bullet",
		Help = "Retrieves transformation for a rigid body", Author = "vux")]
	public unsafe class BulletGetRigidBodyTransformNode : IPluginEvaluate
	{
		[Input("Bodies")]
        protected Pin<RigidBody> FBodies;

		[Output("Position")]
        protected ISpread<SlimDX.Matrix> FTransform;

		public void Evaluate(int SpreadMax)
		{
			if (this.FBodies.PluginIO.IsConnected)
			{
				this.FTransform.SliceCount = this.FBodies.SliceCount;

                List<Matrix4x4> transforms = new List<Matrix4x4>();
                List<Vector3> scaling = new List<Vector3>();

				for (int i = 0; i < SpreadMax; i++)
				{
					RigidBody body = this.FBodies[i];

                    BulletSharp.Matrix m = body.MotionState.WorldTransform;

                    this.FTransform[i] = *((SlimDX.Matrix*)&m);
				}
			}
			else
			{
				this.FTransform.SliceCount = 0;
			}

		}
	}
}
