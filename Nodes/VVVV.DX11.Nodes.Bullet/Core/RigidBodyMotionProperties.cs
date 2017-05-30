using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Rigid body motion properties, used by kinematic/dynamic types
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RigidBodyMotionProperties
    {
        public BulletSharp.Vector3 LinearVelocity;
        public BulletSharp.Vector3 AngularVelocity;
        public bool AllowSleep;

        public static RigidBodyMotionProperties Default
        {
            get
            {
                return new RigidBodyMotionProperties()
                {
                    AllowSleep = true,
                    AngularVelocity = BulletSharp.Vector3.Zero,
                    LinearVelocity = BulletSharp.Vector3.Zero
                };
            }
        }
    }
}
