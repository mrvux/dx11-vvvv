using System;
using System.Collections.Generic;
using System.Text;

using BulletSharp;

using SlimDX.Direct3D9;
using VVVV.Bullet.DataTypes;
using VVVV.Internals.Bullet;

namespace VVVV.DataTypes.Bullet
{
	public abstract class AbstractRigidShapeDefinition
	{
		private float mass;

		public abstract int ShapeCount { get; }

		public virtual float Mass
		{
			get { return mass; }
			set { mass = value; }
		}

        public RigidBodyPose Pose;
        public Vector3 Scaling;


		public string CustomString { get; set; }
		public ICloneable CustomObject { get; set; }

		public CollisionShape GetShape(ShapeCustomData sc)
		{
			CollisionShape shape = this.CreateShape();
			shape.LocalScaling = this.Scaling;
			sc.CustomString = this.CustomString;
			if (sc.CustomObject != null) { sc.CustomObject = (ICloneable)this.CustomObject.Clone(); }
			shape.UserObject = sc;
			shape.CalculateLocalInertia(this.Mass);

			return shape;
		}

		protected abstract CollisionShape CreateShape();
	}
}
