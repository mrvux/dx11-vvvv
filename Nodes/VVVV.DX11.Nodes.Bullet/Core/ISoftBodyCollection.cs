using BulletSharp.SoftBody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Interface for object that is a soft body container
    /// </summary>
    public interface ISoftBodyCollection
    {
        /// <summary>
        /// List of soft bodies
        /// </summary>
        List<SoftBody> SoftBodies { get; }
    }
}
