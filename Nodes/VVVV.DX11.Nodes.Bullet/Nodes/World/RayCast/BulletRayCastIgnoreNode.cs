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
	[PluginInfo(Name = "RayCast", Category = "Bullet",Version="Filtered", Author = "vux")]
	public class BulletRayCastFilterNode : IPluginEvaluate
	{
		[Input("World", IsSingle = true)]
        protected Pin<IBulletWorld> FWorld;

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

        [Output("Hit Count")]
        protected ISpread<int> FHitCount;

        private AllHitsRayResultCallback cb;

        List<double> fraction = new List<double>();
        List<Vector3D> position = new List<Vector3D>();
        List<Vector3D> normal = new List<Vector3D>();
        List<RigidBody> body = new List<RigidBody>();
        List<int> qidx = new List<int>();

        public void Evaluate(int dummy)
		{
			if (this.FWorld.IsConnected)
			{
                fraction.Clear();
                position.Clear();
                normal.Clear();
                body.Clear();
                qidx.Clear();

                //Ignore slice count for excluded bodies, as 0 is allowed (means we do a full search)
                int spreadMax = SpreadUtils.SpreadMax(FWorld, FFrom, FTo);

                this.FHit.SliceCount = spreadMax;
                this.FHitCount.SliceCount = spreadMax;

                for (int i = 0; i < spreadMax; i++)
				{
					Vector3 from = this.FFrom[i].ToBulletVector();
					Vector3 to = this.FTo[i].ToBulletVector();

                    if (cb == null)
                    {
                        cb = new AllHitsRayResultCallback(from, to);
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

                        for (int h = 0; h < cb.HitFractions.Count; h++)
                        {
                            RigidBody rb = (RigidBody)cb.CollisionObjects[h];

                            BodyCustomData bd = (BodyCustomData)rb.UserObject;

                            if (cb.HitFractions[h] < minfrac && !this.FExcludedBody.Contains(rb))
                            {
                                closest = rb;
                                minidx = h;
                            }
                        }

                        if (closest != null)
                        {
                            this.FHit[i] = true;

                            Vector3 diff = to - from;
                            Vector3 inter = from + diff * cb.HitFractions[minidx];

                            position.Add(inter.ToVVVVector());
                            fraction.Add(cb.HitFractions[minidx]);
                            normal.Add(cb.HitNormalWorld[minidx].ToVVVVector());
                            body.Add(closest);
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

                this.FHitFraction.SliceCount = fraction.Count;
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
                this.FHitCount.SliceCount = 0;
                this.FQueryIndex.SliceCount = 0;
			}

		}
	}
}
