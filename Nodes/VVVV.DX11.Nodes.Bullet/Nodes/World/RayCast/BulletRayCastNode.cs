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
        protected Pin<IBulletWorld> FWorld;

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

        private List<double> fraction = new List<double>();
        private List<Vector3D> position = new List<Vector3D>();
        private List<Vector3D> normal = new List<Vector3D>();
        private List<RigidBody> body = new List<RigidBody>();
        private List<int> qidx = new List<int>();

        public void Evaluate(int SpreadMax)
		{
			if (this.FWorld.IsConnected && this.FWorld[0] != null)
			{
				this.FHit.SliceCount = SpreadMax;

                this.fraction.Clear();
                this.position.Clear();
                this.normal.Clear();
                this.body.Clear();
                this.qidx.Clear();

				for (int i = 0; i < SpreadMax; i++)
				{
					Vector3 from = this.FFrom[i].ToBulletVector();
					Vector3 to = this.FTo[i].ToBulletVector();

					ClosestRayResultCallback cb =
						new ClosestRayResultCallback(ref from,ref to);

					this.FWorld[0].World.RayTest(from, to, cb);

					if (cb.HasHit)
					{
						this.FHit[i] = true;
						fraction.Add(cb.ClosestHitFraction);
						position.Add(cb.HitPointWorld.ToVVVVector());
						normal.Add(cb.HitNormalWorld.ToVVVVector());
						body.Add((RigidBody)cb.CollisionObject);
						qidx.Add(i);
					}
					else
					{
						this.FHit[i] = false;
					}
				}

                this.FHit.SliceCount = fraction.Count;
                this.FHitNormal.SliceCount = fraction.Count;
                this.FHitPosition.SliceCount = fraction.Count;
                this.FQueryIndex.SliceCount = fraction.Count;
                this.FBody.SliceCount = fraction.Count;

                for (int i = 0; i < fraction.Count; i++)
                {
                    this.FHitFraction[i] = fraction[i];
                    this.FHitNormal[i] = normal[i];
                    this.FHitPosition[i] = position[i];
                    this.FBody[i] = body[i];
                    this.FQueryIndex[i] = qidx[i];
                }
			}
			else
			{
				this.FHit.SliceCount = 0;
				this.FHitFraction.SliceCount = 0;
				this.FHitPosition.SliceCount = 0;
                this.FHitNormal.SliceCount = 0;
                this.FBody.SliceCount = 0;
                this.FQueryIndex.SliceCount = 0;
			}

		}
	}
}
