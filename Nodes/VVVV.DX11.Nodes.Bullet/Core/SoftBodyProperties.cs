using BulletSharp.SoftBody;
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
    public struct SoftBodyProperties
    {
        public AeroModel AeroModel;
        public bool IsVolumeMass;
        public float Mass;
        public float DampingCoefficient;
        public float DragCoefficient;
        public float DynamicFrictionCoefficient;
        public float PressureCoefficient;
        public float VolumeConservationCoefficient;
        public float LiftCoefficient;
        public float RigidContactHardness;
        public float SoftContactHardness;
        public float AnchorHardness;

        public static SoftBodyProperties Default
        {
            get
            {
                return new SoftBodyProperties()
                {
                    AeroModel = AeroModel.VPoint,
                    IsVolumeMass = false,
                    Mass = 1.0f,
                    DampingCoefficient = 0.0f,
                    DragCoefficient = 0.0f,
                    DynamicFrictionCoefficient=0.0f,
                    PressureCoefficient = 0.0f,
                    VolumeConservationCoefficient = 0.0f,
                    LiftCoefficient = 1.0f,
                    SoftContactHardness = 1.0f,
                    AnchorHardness = 1.0f
                };
            }
        }
    }
}
