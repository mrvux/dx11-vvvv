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
	[PluginInfo(Name="GetRigidBodyCustom",Category="Bullet",
		Help = "Retrieves custom details for a rigid body", Author = "vux")]
	public class BulletGetRigidBodyCustomNode : IPluginEvaluate
	{
		[Input("Bodies")]
        protected Pin<RigidBody> FBodies;

		[Output("Custom")]
        protected ISpread<string> FCustom;

		[Import()]
        protected ILogger FLogger;

		public void Evaluate(int SpreadMax)
		{
			if (this.FBodies.IsConnected)
			{
				this.FCustom.SliceCount = this.FBodies.SliceCount;

				for (int i = 0; i < SpreadMax; i++)
				{
					RigidBody body = this.FBodies[i];
					BodyCustomData bd = (BodyCustomData)body.UserObject;

					this.FCustom[i] = bd.Custom;
				}
			}
			else
			{
				this.FCustom.SliceCount = 0;
			}

		}
	}
}
