using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.DataTypes.Vehicle
{
    public class WheelInfoSettings
    {
        public WheelInfoSettings()
        {
            this.SuspensionStiffness = 20.0f;
            this.WheelsDampingRelaxation = 2.3f;
            this.WheelsDampingCompression = 4.4f;
            this.FrictionSlip = 1000;
            this.RollInfluence = 0.1f;
        }

        public float SuspensionStiffness;
        public float WheelsDampingRelaxation;
        public float WheelsDampingCompression;
        public float FrictionSlip;
        public float RollInfluence;
    }

    public class WheelConstructionSettings
    {
        public float WheelRadius = 0.7f;
        public float WheelWidth = 0.4f;
        public float SuspensionRestLength = 0.6f;
    }
}
