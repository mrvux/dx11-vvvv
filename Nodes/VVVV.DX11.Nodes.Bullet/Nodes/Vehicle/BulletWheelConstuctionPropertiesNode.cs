using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using VVVV.Bullet.Core.Vehicle;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "ConstructionProperties", Category = "Bullet", Version = "Wheel",
        Help = "Construction properties for a vehicle wheel", Author = "vux", Tags ="vehicle")]
    public unsafe class BulleteCreateWheelCstInfoNode : IPluginEvaluate
    {
        [Input("Local Position", DefaultValue = 0.7f)]
        protected ISpread<SlimDX.Vector3> LocalPosition;

        [Input("Wheel Radius", DefaultValue =0.7f)]
        protected ISpread<float> WheelRadius;

        [Input("Wheels Width", DefaultValue =0.4f)]
        protected ISpread<float> WheelWidth;

        [Input("Suspension Rest Length", DefaultValue = 0.6f)]
        protected ISpread<float> SuspensionRestLength;

        [Input("Is Front Wheel", DefaultValue = 1)]
        protected ISpread<bool> isFront;

        [Output("Output")]
        protected ISpread<WheelConstructionProperties> output;

        public void Evaluate(int SpreadMax)
        {
            this.output.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                this.output[i] = new WheelConstructionProperties()
                {
                    WheelRadius = WheelRadius[i],
                    localPosition  = LocalPosition[i],
                    SuspensionRestLength = SuspensionRestLength[i],
                    WheelWidth = WheelWidth[i],
                    isFrontWheel = isFront[i],
                };
            }
        }
    }
}
