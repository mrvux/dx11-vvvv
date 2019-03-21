using BulletSharp.SoftBody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    public interface ISoftBulletWorld : IBulletWorld, ISoftBodyContainer
    {
        SoftBodyWorldInfo WorldInfo { get; }
    }
}
