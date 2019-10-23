using BulletSharp;
using BulletSharp.SoftBody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    public interface ISoftBodyContainer : ISoftBodyCollection
    {
        int GetNewSoftBodyId();
        void Register(SoftBody body);
        event SoftBodyDeletedDelegate SoftBodyDeleted;
        event WorldResetDelegate WorldHasReset;
    }
}
