using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using VVVV.Bullet.Core;

namespace VVVV.DataTypes.Bullet
{
	public class BoxShapeDefinition : DynamicShapeDefinitionBase
    {
		private float w,h,d;

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
