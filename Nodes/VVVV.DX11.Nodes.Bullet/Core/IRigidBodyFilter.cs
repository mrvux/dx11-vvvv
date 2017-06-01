using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Rigid body filter
    /// </summary>
    public interface IRigidBodyFilter
    {
        /// <summary>
        /// Tells if we want a body to pass filter
        /// </summary>
        /// <param name="body">Body to test</param>
        /// <returns>true if test pass, false otherwise</returns>
        bool Filter(RigidBody body);
    }
}
