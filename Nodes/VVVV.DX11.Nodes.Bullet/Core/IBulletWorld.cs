using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Gets Bullet world details
    /// </summary>
    public interface IBulletWorld
    {
        /// <summary>
        /// Collision dispatcher
        /// </summary>
        Dispatcher Dispatcher { get; }

        /// <summary>
        /// Dynamics world instance
        /// </summary>
        DynamicsWorld World { get; }
    }
}
