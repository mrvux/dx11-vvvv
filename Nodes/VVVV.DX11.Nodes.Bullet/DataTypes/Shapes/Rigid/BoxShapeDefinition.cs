using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class BoxShapeDefinition : AbstractRigidShapeDefinition
	{
		private float w,h,d;

		public override int ShapeCount
		{
			get { return 1; }
		}

		public BoxShapeDefinition(float width, float height, float depth)
		{
			this.w = width / 2.0f;
			this.h = height / 2.0f;
			this.d = depth / 2.0f;
		}


		protected override CollisionShape CreateShape()
		{
			CollisionShape shape = new BoxShape(this.w,this.h,this.d);
			return shape;
		}
	}
}
