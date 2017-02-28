using System;
using System.Collections.Generic;
using System.Text;

using BulletSharp;

using SlimDX.Direct3D9;

using VVVV.Internals.Bullet.EX9;

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

		protected override BulletMesh CreateMesh(Device device)
		{
            return null;// new BulletMesh(Mesh.CreateSphere(device, this.radius, resx, resy));	
		}
	}
}
