using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using VVVV.DataTypes.Bullet;


namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="Cylinder",Category="Bullet",Author="vux")]
	public class BulletCylinderShapeNode : AbstractBulletRigidShapeNode
	{
		[Input("Radius", DefaultValue = 0.5)]
        protected IDiffSpread<float> FRadius;
		
		[Input("Length", DefaultValue = 1.0)]
        protected IDiffSpread<float> FLength;

		public override void Evaluate(int SpreadMax)
		{
			if (this.BasePinsChanged
				|| this.FRadius.IsChanged
				|| this.FLength.IsChanged)
			{
				this.FShapes.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax; i++)
				{
                    CylinderShapeDefinition cyl = new CylinderShapeDefinition(Math.Abs(this.FRadius[i]), Math.Abs(this.FRadius[i]), Math.Abs(this.FLength[i]));
					cyl.Mass = this.FMass[i];
					this.SetBaseParams(cyl, i);
					this.FShapes[i] = cyl;
				}
			}			
		}
	}
}
