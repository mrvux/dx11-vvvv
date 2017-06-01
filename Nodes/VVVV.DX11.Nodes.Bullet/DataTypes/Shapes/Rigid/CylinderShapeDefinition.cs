using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using FeralTic;
using VVVV.Bullet.Core;

namespace VVVV.DataTypes.Bullet
{
	public class CylinderShapeDefinition : DynamicShapeDefinitionBase
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
