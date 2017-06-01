using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using VVVV.Bullet.Core;

namespace VVVV.DataTypes.Bullet
{
    public class PlaneShapeDefinition : RigidShapeDefinitionBase
    {
        private Vector3 normal;
        private float w;

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
