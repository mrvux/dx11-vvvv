using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace VVVV.Bullet.Core.Filters
{
    public class SingleShapeFilter : IRigidBodyFilter
    {
        public BroadphaseNativeType ShapeType = BroadphaseNativeType.StaticPlane;

        public bool Filter(RigidBody body)
        {
            return body.CollisionShape.ShapeType == this.ShapeType;
        }
    }
}
