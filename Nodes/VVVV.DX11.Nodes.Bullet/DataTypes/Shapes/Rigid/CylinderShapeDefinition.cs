using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class CylinderShapeDefinition : AbstractRigidShapeDefinition
	{
		private float hw,hh,hd;

		public CylinderShapeDefinition(float hwidth, float hheight, float hdepth)
		{
			this.hw = hwidth;
			this.hh = hheight;
			this.hd = hdepth;
		}

		public override int ShapeCount
		{
			get { return 1; }
		}


		protected override CollisionShape CreateShape()
		{
			//Cylinder are around Z axis in vvvv
			//If we need Y/X axis, we can rotate, so i use Z
			CollisionShape shape = new CylinderShapeZ(this.hw,this.hh,this.hd);
			return shape;
		}
	}
}
