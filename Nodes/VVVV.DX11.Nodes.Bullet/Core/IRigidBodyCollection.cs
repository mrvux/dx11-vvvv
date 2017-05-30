using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Interface for an element that contains rigid bodies
    /// </summary>
    public interface IRigidBodyCollection
    {
        /// <summary>
        /// List of rigies bodies
        /// </summary>
        List<RigidBody> RigidBodies { get; }
    }
}
