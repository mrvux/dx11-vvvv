using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.DataTypes.Bullet;

using BulletSharp;
using System.ComponentModel.Composition;

using VVVV.Core.Logging;
using VVVV.Internals.Bullet;
using VVVV.Utils.VMath;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "GetContactDetails", Category = "Bullet", Author = "vux")]
	public class BulletGetContactDetailsNode : IPluginEvaluate
	{
		[Input("World")]
        protected Pin<IBulletWorld> FWorld;

		[Output("Body 1")]
        protected ISpread<RigidBody> FBody1;

		[Output("Body 2")]
        protected ISpread<RigidBody> FBody2;

		[Output("Contact Points")]
        protected ISpread<ISpread<ManifoldPoint>> FContactPoints;

		[Import()]
        protected ILogger FLogger;

		public void Evaluate(int SpreadMax)
		{

			if (this.FWorld.PluginIO.IsConnected)
			{
				int contcnt = this.FWorld[0].Dispatcher.NumManifolds;
				this.FBody1.SliceCount = contcnt;
				this.FBody2.SliceCount = contcnt;
				this.FContactPoints.SliceCount = contcnt;

				for (int i = 0; i < contcnt; i++)
				{
					PersistentManifold pm = this.FWorld[0].Dispatcher.GetManifoldByIndexInternal(i);
                    RigidBody b1 = RigidBody.Upcast((CollisionObject)pm.Body0);
                    RigidBody b2 = RigidBody.Upcast((CollisionObject)pm.Body1);

					this.FBody1[i] = b1;
					this.FBody2[i] = b2;

					this.FContactPoints[i].SliceCount = pm.NumContacts;
					for (int j = 0; j < pm.NumContacts; j++)
					{
						this.FContactPoints[i][j] = pm.GetContactPoint(j);
					}

				}
			}
			else
			{
				FBody1.SliceCount = 0;
				FBody2.SliceCount = 0;
			}
		}


	}
}
