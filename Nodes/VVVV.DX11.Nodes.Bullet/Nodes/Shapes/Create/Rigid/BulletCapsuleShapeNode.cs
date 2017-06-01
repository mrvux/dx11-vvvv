using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using VVVV.DataTypes.Bullet;
using FeralTic;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="Capsule",Category="Bullet",Author="vux")]
	public class BulletCapsuleShapeNode : AbstractBulletRigidDynamicShapeNode
    {
		[Input("Radius", DefaultValue = 0.5)]
        protected IDiffSpread<float> FRadius;
		
		[Input("Height", DefaultValue = 1.0)]
        protected IDiffSpread<float> FHeight;

        [Input("Axis", DefaultEnumEntry = "Y")]
        protected IDiffSpread<Axis> FAxis;

        public override void Evaluate(int SpreadMax)
		{
			if (this.BasePinsChanged
				|| this.FRadius.IsChanged
				|| this.FHeight.IsChanged
                || this.FAxis.IsChanged)
			{
				this.FShapes.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax; i++)
				{
                    CapsuleShapeDefinition cyl = new CapsuleShapeDefinition(Math.Abs(this.FRadius[i]), Math.Abs(this.FHeight[i]), FAxis[i]);
					cyl.Mass = this.FMass[i];
					this.SetBaseParams(cyl, i);
					this.FShapes[i] = cyl;
				}
			}			
		}
	}
}
