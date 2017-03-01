using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

using BulletSharp;
using VVVV.Internals.Bullet;


namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "RayCast", Category = "Bullet",Version="Filtered", Author = "vux")]
	public class BulletRayCastFilterNode : IPluginEvaluate
	{
		[Input("World", IsSingle = true)]
        protected Pin<BulletRigidSoftWorld> FWorld;

        [Input("Excluded Bodies")]
        protected ISpread<RigidBody> FExcludedBody;

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

        [Output("Hit Count")]
        protected ISpread<int> FHitCount;

        private CollisionWorld.AllHitsRayResultCallback cb;

        List<double> fraction = new List<double>();
        List<Vector3D> position = new List<Vector3D>();
        List<Vector3D> normal = new List<Vector3D>();
        List<RigidBody> body = new List<RigidBody>();
        List<int> bodyid = new List<int>();
        List<int> qidx = new List<int>();

        public void Evaluate(int SpreadMax)
		{


			if (this.FWorld.PluginIO.IsConnected)
			{
				this.FHit.SliceCount = SpreadMax;
                this.FHitCount.SliceCount = SpreadMax;

                fraction.Clear();
                position.Clear();
                normal.Clear();
                body.Clear();
                bodyid.Clear();
                qidx.Clear();
                

				for (int i = 0; i < SpreadMax; i++)
				{
					Vector3 from = this.FFrom[i].ToBulletVector();
					Vector3 to = this.FTo[i].ToBulletVector();

                    if (cb == null)
                    {
                        cb = new CollisionWorld.AllHitsRayResultCallback(from, to);
                    }

                    cb.HitFractions.Clear();
                    cb.HitNormalWorld.Clear();
                    cb.HitPointWorld.Clear();
                    cb.CollisionObjects.Clear();

					this.FWorld[0].World.RayTest(from, to, cb);

					if (cb.HasHit)
					{
                        this.FHitCount[i] = cb.HitFractions.Count;

                        float minfrac = float.MaxValue;
                        RigidBody closest = null;
                        int minidx = 0;

                        for (int h = 0; h < cb.HitFractions.Count; h++ )
                        {
                            RigidBody rb = (RigidBody)cb.CollisionObjects[h];
                            if (cb.HitFractions[h] < minfrac && !this.FExcludedBody.Contains(rb))
                            {
                                closest = rb;
                                minidx = h;
                            }
                        }

                        if (closest != null)
                        {
                            this.FHit[i] = true;
                            BodyCustomData bd = (BodyCustomData)closest.UserObject;
                            fraction.Add(cb.HitFractions[minidx]);
                            position.Add(cb.HitPointWorld[minidx].ToVVVVector());
                            normal.Add(cb.HitNormalWorld[minidx].ToVVVVector());
                            body.Add(closest);
                            bodyid.Add(bd.Id);
                            qidx.Add(i);
                        }
                        else
                        {
                            this.FHit[i] = false;
                        }
					}
					else
					{
						this.FHit[i] = false;
                        this.FHitCount[i] = 0;
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
                this.FHitCount.SliceCount = 0;
			}

		}
	}
}
