using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
    public class PlaneShapeDefinition : AbstractRigidShapeDefinition
    {
        private Vector3 normal;
        private float w;

        public override int ShapeCount
        {
            get { return 1; }
        }

        public PlaneShapeDefinition(Vector3 normal, float w)
        {
            this.normal = normal;
            this.w = w;
        }

        protected override CollisionShape CreateShape()
        {
            CollisionShape shape = new StaticPlaneShape(this.normal, this.w);
            return shape;
        }
    }
}
