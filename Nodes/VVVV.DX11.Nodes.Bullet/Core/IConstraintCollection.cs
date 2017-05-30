using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Interface for an element that contains constraints
    /// </summary>
    public interface IConstraintCollection
    {
        /// <summary>
        /// List of constraints
        /// </summary>
        List<TypedConstraint> Constraints { get; }
    }
}
