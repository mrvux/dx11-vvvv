using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core.Filters
{
    public class MinimumAgeCollisionFilter : IRigidBodyFilter
    {
        public float MinimumAge = 0.0f;

        public bool Filter(RigidBody body)
        {
            BodyCustomData data = (BodyCustomData)body.UserObject;
            return data.LifeTime > MinimumAge;
        }
    }
}
