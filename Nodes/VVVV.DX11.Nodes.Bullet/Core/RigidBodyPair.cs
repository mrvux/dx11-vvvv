using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Simple rigid body pair, this is used to build constraints
    /// </summary>
    public sealed class RigidBodyPair
    {
        public RigidBody body1;
        public RigidBody body2;
        public bool collideConnected;
    }
}
