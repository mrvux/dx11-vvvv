using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class ConvexHullShapeDefinition : AbstractRigidShapeDefinition
	{
		private Vector3[] vertices;

		public ConvexHullShapeDefinition(Vector3[] vertices)
		{
			this.vertices = vertices;
		}

		public override int ShapeCount
		{
			get { return 1; }
		}

		protected override CollisionShape CreateShape()
		{
			ConvexHullShape shape = new ConvexHullShape(this.vertices);
			return shape;
		}
	}
}
