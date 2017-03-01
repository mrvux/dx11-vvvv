using System;
using System.Collections.Generic;
using System.Text;

using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class SphereShapeDefinition : AbstractRigidShapeDefinition
	{
		private float radius;

		public SphereShapeDefinition(float radius)
		{
			this.radius = radius;
		}

		public override int ShapeCount
		{
			get { return 1; }
		}

		protected override CollisionShape CreateShape()
		{
			CollisionShape shape = new SphereShape(this.radius);
			//shape.CalculateLocalInertia(this.Mass);
			return shape;
		}
	}
}
