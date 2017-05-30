using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Soft body custom data
    /// </summary>
    public class SoftBodyCustomData : ObjectCustomData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Object id</param>
        public SoftBodyCustomData(int id) : base(id) { }

        /// <summary>
        /// Tells if object has UV
        /// </summary>
        public bool HasUV { get; set; }

        /// <summary>
        /// Uvs set for object
        /// </summary>
        public float[] UV { get; set; }
    }
}
