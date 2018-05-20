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
	[PluginInfo(Name="GetBodyId",Category="Bullet",
		Help = "Retrieves id for a rigid body", Author = "vux")]
	public unsafe class BulletGetBodyIdTransformNode : IPluginEvaluate
	{
		[Input("Bodies")]
        protected Pin<RigidBody> bodies;

		[Output("Id")]
        protected ISpread<int> id;

		public void Evaluate(int SpreadMax)
		{
			if (this.bodies.IsConnected)
			{
                this.id.SliceCount = this.bodies.SliceCount;

                var ids = this.id.Stream.Buffer;

				for (int i = 0; i < SpreadMax; i++)
				{
					RigidBody body = this.bodies[i];
                    BodyCustomData bd = (BodyCustomData)body.UserObject;
                    ids[i] = bd.Id;
                }
                this.id.Flush(true);
			}
			else
			{
				this.id.SliceCount = 0;
			}

		}
	}
}
