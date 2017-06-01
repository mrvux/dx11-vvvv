using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using VVVV.Bullet.Core;

namespace VVVV.DataTypes.Bullet
{
	public class ConvexHullShapeDefinition : DynamicShapeDefinitionBase
	{
		private Vector3[] vertices;

		public ConvexHullShapeDefinition(Vector3[] vertices)
		{
			this.vertices = vertices;
		}

		protected override CollisionShape CreateShape()
		{
			ConvexHullShape shape = new ConvexHullShape(this.vertices);
			return shape;
		}
	}
}
