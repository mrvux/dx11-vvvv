using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Rigid body basic properties, common to every rigid body type
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RigidBodyProperties
    {
        public float Friction;
        public float Restitution;
        public float RollingFriction;
        public bool IsActive;
        public bool HasContactResponse;
        public bool DebugViewEnabled;

        public static RigidBodyProperties Default
        {
            get
            {
                return new RigidBodyProperties()
                {
                    Friction = 0.1f,
                    HasContactResponse = true,
                    IsActive = true,
                    Restitution = 0.5f,
                    RollingFriction = 0.1f,
                    DebugViewEnabled = true,
                };
            }
        }
    }
}
