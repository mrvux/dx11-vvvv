using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    public interface IConstraintContainer : IConstraintCollection
    {
        int GetNewConstraintId();
        void Register(TypedConstraint body);
        event ConstraintDeletedDelegate ConstraintDeleted;
        event WorldResetDelegate WorldHasReset;
    }
}
