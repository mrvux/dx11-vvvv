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
	[PluginInfo(Name="GetBodyTransform",Category="Bullet",
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
                var outputBuffer = this.FTransform.Stream.Buffer;

				for (int i = 0; i < SpreadMax; i++)
				{
					RigidBody body = this.FBodies[i];

                    BulletSharp.Matrix m = body.MotionState.WorldTransform;

                    outputBuffer[i] = *((SlimDX.Matrix*)&m);
				}
                this.FTransform.Flush(true);
			}
			else
			{
				this.FTransform.SliceCount = 0;
			}

		}
	}
}
