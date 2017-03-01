using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using FeralTic;

namespace VVVV.DataTypes.Bullet
{
	public class CylinderShapeDefinition : AbstractRigidShapeDefinition
	{
		private float radius;
        private float halfLength;
        private Axis axis;

		public CylinderShapeDefinition(float radius, float length, Axis axis)
		{
            this.radius = radius;
            this.halfLength = length * 0.5f;
            this.axis = axis;
		}

		public override int ShapeCount
		{
			get { return 1; }
		}


		protected override CollisionShape CreateShape()
		{
            if (axis == Axis.X)
            {
                return new CylinderShapeX(this.radius, this.halfLength, this.radius);
            }
            else if (axis == Axis.Y)
            {
                return new CylinderShape(this.radius, this.halfLength, this.radius);
            }
            else
            {
                return new CylinderShapeZ(this.radius, this.halfLength, this.radius);
            }
		}
	}
}
