using System;
using System.Collections.Generic;
using System.Text;

using BulletSharp;
using VVVV.Bullet.Core;

namespace VVVV.DataTypes.Bullet
{
	public class SphereShapeDefinition : DynamicShapeDefinitionBase
	{
		private float radius;

		public SphereShapeDefinition(float radius)
		{
			this.radius = radius;
		}

		protected override CollisionShape CreateShape()
		{
			CollisionShape shape = new SphereShape(this.radius);
			return shape;
		}
	}
}
