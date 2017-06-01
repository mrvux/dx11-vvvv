using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace VVVV.Bullet.Core.Filters
{
    public class StaticOnlyFilter : IRigidBodyFilter
    {
        public bool Filter(RigidBody body)
        {
            return body.IsStaticObject;
        }
    }

    public class DynamicOnlyFilter : IRigidBodyFilter
    {
        public bool Filter(RigidBody body)
        {
            return !body.IsStaticOrKinematicObject;
        }
    }
}
