using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.DataTypes.Bullet;
using VVVV.Internals.Bullet;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	
	public abstract class AbstractSingleConstraintNode<T> : IPluginEvaluate where T: TypedConstraint
	{
		[Input("World", IsSingle = true,Order=1)]
		protected Pin<BulletSoftWorldContainer> FWorld;

		[Input("Bodies",Order=2)]
        protected Pin<RigidBody> FBodies;

		[Input("Custom", Order = 100)]
        protected ISpread<string> FCustom;

		[Input("Do Create",IsBang=true,Order=1000)]
        protected ISpread<bool> FDoCreate;

		protected abstract T CreateConstraint(RigidBody body, int slice);

		public void Evaluate(int SpreadMax)
		{
			if (FBodies.PluginIO.IsConnected && this.FWorld.PluginIO.IsConnected)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FDoCreate[i])
					{
						T cst = this.CreateConstraint(this.FBodies[i], i);

						ConstraintCustomData cust = new ConstraintCustomData(this.FWorld[0].GetNewConstraintId());
						cust.Custom = this.FCustom[i];
						cust.IsSingle = true;

						cst.UserObject = cust;
						this.FWorld[0].Register(cst);

					}
				}
			}
		}
	}
}
