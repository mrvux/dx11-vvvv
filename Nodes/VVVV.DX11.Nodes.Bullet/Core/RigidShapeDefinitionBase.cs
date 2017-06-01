using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Internals.Bullet;

namespace VVVV.Bullet.Core
{
    public abstract class RigidShapeDefinitionBase
    {
        public RigidBodyPose Pose;
        public Vector3 Scaling;

        public virtual int ShapeCount
        {
            get { return 1; }
        }

        public string CustomString { get; set; }

        public virtual CollisionShape GetShape(ShapeCustomData sc)
        {
            CollisionShape shape = this.CreateShape();
            shape.LocalScaling = this.Scaling;
            sc.CustomString = this.CustomString;

            shape.UserObject = sc;
            

            return shape;
        }

        protected abstract CollisionShape CreateShape();
    }

    public abstract class DynamicShapeDefinitionBase : RigidShapeDefinitionBase
    {
        private float mass;

        public virtual float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        public override CollisionShape GetShape(ShapeCustomData sc)
        {
            var result = base.GetShape(sc);
            result.CalculateLocalInertia(this.Mass);
            return result;
        }
    }
}
