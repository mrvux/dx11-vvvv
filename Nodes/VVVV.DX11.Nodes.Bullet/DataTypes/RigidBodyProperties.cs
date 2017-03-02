using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.DataTypes
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

    /// <summary>
    /// Rigid body object information, contains lifetime data
    /// </summary>
    public class RigidBodyObjectInformation
    {
        private readonly int objectId;

        public int ObjectId
        {
            get { return this.objectId; }
        }

        public RigidBodyObjectInformation(int objectId)
        {
            this.objectId = objectId;
        }
    }
}
