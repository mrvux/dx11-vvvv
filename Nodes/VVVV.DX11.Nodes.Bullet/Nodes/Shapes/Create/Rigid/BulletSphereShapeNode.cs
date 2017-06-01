using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="Sphere",Category="Bullet",Author="vux")]
	public class BulletSphereShapeNode : AbstractBulletRigidDynamicShapeNode
    {
		[Input("Radius", DefaultValue = 0.5)]
        protected IDiffSpread<float> FRadius;

		public override void Evaluate(int SpreadMax)
		{
			if (this.BasePinsChanged || this.FRadius.IsChanged)
			{
				this.FShapes.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax; i++)
				{
                    SphereShapeDefinition sphere = new SphereShapeDefinition(Math.Abs(this.FRadius[i]));
					sphere.Mass = this.FMass[i];
					this.SetBaseParams(sphere, i);
					this.FShapes[i] = sphere;
				}
			}			
		}
	}
}
