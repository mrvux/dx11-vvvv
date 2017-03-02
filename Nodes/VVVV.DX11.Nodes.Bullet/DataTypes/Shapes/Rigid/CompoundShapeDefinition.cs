using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using VVVV.Internals.Bullet;

namespace VVVV.DataTypes.Bullet
{
	public class CompoundShapeDefinition : AbstractRigidShapeDefinition
	{
		private List<AbstractRigidShapeDefinition> children;

		public CompoundShapeDefinition(List<AbstractRigidShapeDefinition> children)
		{
			this.children = children;
		}

		public List<AbstractRigidShapeDefinition> Children
		{
			get { return children; }
		}

		
		public override float Mass
		{
			get
			{
				float mass = 0;
				foreach (AbstractRigidShapeDefinition def in this.children)
				{
					mass += def.Mass;
				}
				return mass;
			}
			set
			{
				base.Mass = value;
			}
		}

		public override int ShapeCount
		{
			get 
			{
				int cnt = 0;
				foreach (AbstractRigidShapeDefinition def in this.children)
				{
					cnt += def.ShapeCount;
				}
				return cnt;
			}
		}

		protected override CollisionShape CreateShape()
		{
			CompoundShape shape = new CompoundShape();
			foreach (AbstractRigidShapeDefinition shapedef in this.children)
			{
				ShapeCustomData sc = new ShapeCustomData();
				sc.Id = 0;
				sc.ShapeDef = shapedef;
				shape.AddChildShape((Matrix)shapedef.Pose , shapedef.GetShape(sc));
			}
			return shape;
		}
	}
}

