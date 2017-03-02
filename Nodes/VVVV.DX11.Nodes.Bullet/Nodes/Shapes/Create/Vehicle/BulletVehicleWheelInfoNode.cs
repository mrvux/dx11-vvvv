using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;
using VVVV.Bullet.DataTypes.Vehicle;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "WheelInfo", Category = "Bullet", Version = "", Author = "vux")]
    public class BulletBehicleWheelInfoNode : IPluginEvaluate
    {
        [Input("Suspension Stiffness", DefaultValue = 20.0)]
        protected IDiffSpread<float> suspensionStiffness;

        [Input("Wheels Damping Relaxation", DefaultValue = 2.3)]
        protected IDiffSpread<float> wheelsDampingRelaxation;

        [Input("Wheels Damping Compression", DefaultValue = 4.4)]
        protected IDiffSpread<float> wheelsDampingCompression;

        [Input("Friction Slip", DefaultValue =1000)]
        protected IDiffSpread<int> frictionSlip;

        [Input("Roll Influence", DefaultValue = 0.1)]
        protected IDiffSpread<float> rollInfluence;

        [Output("Output")]
        protected ISpread<WheelInfoSettings> output;

        public void Evaluate(int SpreadMax)
        {
            if (SpreadUtils.AnyChanged(this.suspensionStiffness, this.wheelsDampingCompression, this.wheelsDampingRelaxation, this.frictionSlip, this.rollInfluence))
            {
                this.output.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    this.output[i] = new WheelInfoSettings()
                    {
                        FrictionSlip = this.frictionSlip[i],
                        RollInfluence = this.rollInfluence[i],
                        SuspensionStiffness = this.suspensionStiffness[i],
                        WheelsDampingCompression = this.wheelsDampingCompression[i],
                        WheelsDampingRelaxation = this.wheelsDampingRelaxation[i]
                    };
                }
            }
            
        }
    }
}
