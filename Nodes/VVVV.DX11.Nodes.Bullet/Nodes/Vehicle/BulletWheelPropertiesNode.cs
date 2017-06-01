using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using VVVV.Bullet.Core.Vehicle;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "WheelProperties", Category = "Bullet", Version ="", Help = "Sets properties for a bullet wheel", Author = "vux", Tags ="vehicle")]
    public unsafe class BulleteCreateWheelInfoNode : IPluginEvaluate
    {
        [Input("Suspension Stiffness", DefaultValue =20.0f)]
        protected ISpread<float> SuspensionStiffness;

        [Input("Wheels Damping Relaxation", DefaultValue =2.3f)]
        protected ISpread<float> WheelsDampingRelaxation;

        [Input("Wheels Damping Compression", DefaultValue = 4.4f)]
        protected ISpread<float> WheelsDampingCompression;

        [Input("Friction Slip", DefaultValue = 1000)]
        protected ISpread<float> FrictionSlip;

        [Input("Roll Influence", DefaultValue = 0.1f)]
        protected ISpread<float> RollInfluence;

        [Output("Output")]
        protected ISpread<WheelProperties> output;

        public void Evaluate(int SpreadMax)
        {
            this.output.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                this.output[i] = new WheelProperties()
                {
                    FrictionSlip = FrictionSlip[i],
                    RollInfluence = RollInfluence[i],
                    SuspensionStiffness = SuspensionStiffness[i],
                    WheelsDampingCompression = WheelsDampingCompression[i],
                    WheelsDampingRelaxation = WheelsDampingRelaxation[i]
                };
            }
        }
    }
}
