using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace VVVV.Bullet.Core.Filters
{
    public class AndFilter : IRigidBodyFilter
    {
        public IRigidBodyFilter First;
        public IRigidBodyFilter Second;

        public bool Filter(RigidBody body)
        {
            bool first = First != null ? First.Filter(body) : true;
            bool second = Second != null ? Second.Filter(body) : true;
            return first && second;
        }
    }

    public class OrFilter : IRigidBodyFilter
    {
        public IRigidBodyFilter First;
        public IRigidBodyFilter Second;

        public bool Filter(RigidBody body)
        {
            bool first = First != null ? First.Filter(body) : true;
            bool second = Second != null ? Second.Filter(body) : true;
            return first || second;
        }
    }

    public class NotFilter : IRigidBodyFilter
    {
        public IRigidBodyFilter filter;

        public bool Filter(RigidBody body)
        {
            return filter != null ? !filter.Filter(body) : true; 
        }
    }

}
