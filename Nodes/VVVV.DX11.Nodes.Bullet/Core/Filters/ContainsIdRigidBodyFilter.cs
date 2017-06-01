using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Bullet.Core.Filters
{
    public class ContainsIdRigidBodyFilter : IRigidBodyFilter
    {
        public ISpread<int> IdList;

        public bool Filter(RigidBody body)
        {
            if (IdList == null)
                return false;

            BodyCustomData data = (BodyCustomData)body.UserObject;
            return IdList.Contains(data.Id);
        }
    }
}
