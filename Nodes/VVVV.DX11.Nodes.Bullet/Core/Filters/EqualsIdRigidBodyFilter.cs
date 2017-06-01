using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace VVVV.Bullet.Core.Filters
{
    public class EqualsIdRigidBodyFilter : IRigidBodyFilter
    {
        public int? Id;

        public bool Filter(RigidBody body)
        {
            BodyCustomData data = (BodyCustomData)body.UserObject;
            return Id.HasValue ? data.Id == Id : false;
        }
    }
}
