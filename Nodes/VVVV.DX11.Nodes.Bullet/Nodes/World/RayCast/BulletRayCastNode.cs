using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

using BulletSharp;
using VVVV.Internals.Bullet;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "RayCast", Category = "Bullet", Author = "vux")]
	public class BulletRayCastNode : IPluginEvaluate
	{
		[Input("World", IsSingle = true)]
        protected Pin<BulletSoftWorldContainer> FWorld;

		[Input("From")]
        protected ISpread<Vector3D> FFrom;

		[Input("To")]
        protected ISpread<Vector3D> FTo;

		[Output("Hit")]
        protected ISpread<bool> FHit;

		[Output("Hit Fraction")]
        protected ISpread<double> FHitFraction;

		[Output("Hit Position")]
        protected ISpread<Vector3D> FHitPosition;

		[Output("Hit Normal")]
        protected ISpread<Vector3D> FHitNormal;

		[Output("Query Index")]
        protected ISpread<int> FQueryIndex;

		[Output("Body")]
        protected ISpread<RigidBody> FBody;

		[Output("Body Id")]
        protected ISpread<int> FId;

		public void Evaluate(int SpreadMax)
		{


			if (this.FWorld.PluginIO.IsConnected)
			{
				this.FHit.SliceCount = SpreadMax;

				List<double> fraction = new List<double>();
				List<Vector3D> position = new List<Vector3D>();
				List<Vector3D> normal = new List<Vector3D>();
				List<RigidBody> body = new List<RigidBody>();
				List<int> bodyid = new List<int>();
				List<int> qidx = new List<int>();

				for (int i = 0; i < SpreadMax; i++)
				{
					Vector3 from = this.FFrom[i].ToBulletVector();
					Vector3 to = this.FTo[i].ToBulletVector();
					CollisionWorld.ClosestRayResultCallback cb =
						new CollisionWorld.ClosestRayResultCallback(from,to );
                    //cb.CollisionFilterMask = 
 
					this.FWorld[0].World.RayTest(from, to, cb);

					if (cb.HasHit)
					{
						this.FHit[i] = true;
						BodyCustomData bd = (BodyCustomData)cb.CollisionObject.UserObject;
						fraction.Add(cb.ClosestHitFraction);
						position.Add(cb.HitPointWorld.ToVVVVector());
						normal.Add(cb.HitNormalWorld.ToVVVVector());
						body.Add((RigidBody)cb.CollisionObject);
						bodyid.Add(bd.Id);
						qidx.Add(i);
					}
					else
					{
						this.FHit[i] = false;
					}
				}

				this.FId.AssignFrom(bodyid);
				this.FHitFraction.AssignFrom(fraction);
				this.FHitNormal.AssignFrom(normal);
				this.FHitPosition.AssignFrom(position);
				this.FQueryIndex.AssignFrom(qidx);
				this.FBody.AssignFrom(body);
			}
			else
			{
				this.FHit.SliceCount = 0;
				this.FId.SliceCount = 0;
				this.FHitFraction.SliceCount = 0;
				this.FHitPosition.SliceCount = 0;
			}

		}
	}
}
