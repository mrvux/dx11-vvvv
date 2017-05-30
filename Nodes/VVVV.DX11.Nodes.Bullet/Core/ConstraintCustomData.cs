using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Custom data to assign to a body constraint
    /// </summary>
    public class ConstraintCustomData : ObjectCustomData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Constraint id</param>
        public ConstraintCustomData(int id) : base(id) { }

        /// <summary>
        /// Is constraint single (eg : applied on pair)
        /// </summary>
        public bool IsSingle { get; set; }
    }
}
