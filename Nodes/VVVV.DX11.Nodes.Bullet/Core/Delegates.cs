using BulletSharp;
using BulletSharp.SoftBody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    public delegate void WorldResetDelegate();

    public delegate void RigidBodyDeletedDelegate(RigidBody rb, int id);

    public delegate void SoftBodyDeletedDelegate(SoftBody rb, int id);

    public delegate void ConstraintDeletedDelegate(TypedConstraint tc, int id);
}
