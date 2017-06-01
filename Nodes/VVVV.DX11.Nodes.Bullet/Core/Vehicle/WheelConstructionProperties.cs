using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core.Vehicle
{
    public class WheelConstructionProperties
    {
        public float WheelRadius = 0.7f;
        public float WheelWidth = 0.4f;
        public float SuspensionRestLength = 0.6f;
        public float ConnectionHeight = 1.2f;
        public Vector3 wheelDirection = new Vector3(0, -1, 0);
        public Vector3 wheelAxis = new Vector3(-1, 0, 0);
        public Vector3 localPosition;
        public bool isFrontWheel;
    }
}
