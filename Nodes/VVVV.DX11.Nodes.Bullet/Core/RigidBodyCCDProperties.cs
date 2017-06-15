using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Continous collision detection properties
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RigidBodyCCDProperties
    {
        public float CcdMotionThreshold;
        public float CcdSweptSphereRadius;

        public static RigidBodyCCDProperties Default
        {
            get
            {
                return new RigidBodyCCDProperties()
                {
                    CcdMotionThreshold = 0.0f,
                    CcdSweptSphereRadius = 0.2f,
                };
            }
        }
    }
}
