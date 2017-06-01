using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core.Vehicle
{
    public class VehicleSettings
    {
        public int rightIndex = 0;
        public int upIndex = 1;
        public int forwardIndex = 2;
        public float CUBE_HALF_EXTENTS = 1;
        public Vector3 wheelDirectionCS0 = new Vector3(0, -1, 0);
        public Vector3 wheelAxleCS = new Vector3(-1, 0, 0);

        //float gEngineForce = 2000.0f;
        /*float gBreakingForce = 0.0f;

        float maxEngineForce = 2000.0f;//this should be engine/velocity dependent
        float maxBreakingForce = 100.0f;

        float gVehicleSteering = 0.0f;
        float steeringIncrement = 1.0f;
        float steeringClamp = 0.3f;*/
        public float wheelRadius = 0.7f;
        public float wheelWidth = 0.4f;
        public float wheelFriction = 1000;//BT_LARGE_FLOAT;
        public float suspensionStiffness = 20.0f;
        public float suspensionDamping = 2.3f;
        public float suspensionCompression = 4.4f;
        public float rollInfluence = 0.1f;//1.0f;

        public float suspensionRestLength = 0.6f;

    }
}
