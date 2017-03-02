using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace VVVV.Bullet.DataTypes
{
    /// <summary>
    /// A simple rigid body pose
    /// </summary>
    /// 
    [StructLayout(LayoutKind.Sequential)]
    public struct RigidBodyPose
    {
        public Vector3 Position;
        public Quaternion Orientation;


        public static RigidBodyPose Default
        {
            get
            {
                return new RigidBodyPose()
                {
                    Position = new Vector3(0.0f, 0.0f, 0.0f),
                    Orientation = Quaternion.Identity
                };
            }
        }

        public static explicit operator Matrix(RigidBodyPose pose)
        {
            Matrix tr = Matrix.Translation(pose.Position);
            Matrix rot = Matrix.RotationQuaternion(pose.Orientation);
            return Matrix.Multiply(rot, tr);
        }
    }
}
