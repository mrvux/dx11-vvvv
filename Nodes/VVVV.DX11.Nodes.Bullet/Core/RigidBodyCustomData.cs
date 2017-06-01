using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Rigid body custom data
    /// </summary>
    public class BodyCustomData : ObjectCustomData
    {
        public RaycastVehicle Vehicle;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Object id</param>
        public BodyCustomData(int id) : base(id)
        {

        }
    }
}
