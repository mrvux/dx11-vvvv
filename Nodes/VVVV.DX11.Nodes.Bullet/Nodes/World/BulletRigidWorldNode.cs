using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using BulletSharp;
using BulletSharp.SoftBody;
using VVVV.DataTypes.Bullet;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="RigidWorld",Category="Bullet",Author="vux",AutoEvaluate=true)]
	public class BulletRigidWorldNode : IPluginEvaluate
	{
		[Input("Gravity",DefaultValues=new double[] { 0.0,-9.8,0.0 })]
        protected IDiffSpread<Vector3D> FGravity;

		[Input("TimeStep", DefaultValue = 0.01, IsSingle = true)]
        protected IDiffSpread<float> FTimeStep;

		[Input("Iterations", DefaultValue = 8, IsSingle = true)]
        protected IDiffSpread<int> FIterations;

		[Input("Enabled", DefaultValue = 1, IsSingle = true)]
        protected IDiffSpread<bool> FEnabled;

		[Input("Reset", DefaultValue = 0, IsSingle = true,IsBang=true)]
        protected ISpread<bool> FReset;

		[Output("World",IsSingle=true)]
        protected ISpread<BulletRigidWorldContainer> FWorld;

        BulletRigidWorldContainer internalworld = new BulletRigidWorldContainer();

		[Output("Has Reset", DefaultValue = 0, IsSingle = true, IsBang = true)]
        protected ISpread<bool> FHasReset;

		bool bFirstFrame = true;

		public void Evaluate(int SpreadMax)
		{
			bool hasreset = false;

			if (this.FReset[0] || this.bFirstFrame)
			{
				this.FWorld[0] = this.internalworld;
				this.bFirstFrame = false;
				if (this.internalworld.Created)
				{
					this.internalworld.Destroy();
				}
				this.internalworld.Create();

				hasreset = true;
			}

			if (this.FGravity.IsChanged || hasreset)
			{
				Vector3D g = this.FGravity[0];
				this.internalworld.SetGravity((float)g.x, (float)g.y, (float)g.z);
				this.internalworld.World.Gravity = new BulletSharp.Vector3((float)g.x, (float)g.y, (float)g.z);
			}

			if (this.FEnabled.IsChanged || hasreset)
			{
				this.internalworld.Enabled = this.FEnabled[0];
			}

			if (this.FTimeStep.IsChanged || hasreset)
			{
				this.internalworld.TimeStep = this.FTimeStep[0];
			}

			if (this.FIterations.IsChanged || hasreset)
			{
				this.internalworld.Iterations = this.FIterations[0];
			}

			if (this.internalworld.Enabled)
			{
				this.internalworld.ProcessDelete(this.FTimeStep[0]);
				this.internalworld.Step();
				//this.internalworld.WorldInfo.SparseSdf.GarbageCollect();
			}

			this.FHasReset[0] = hasreset;
		}
	}
}
