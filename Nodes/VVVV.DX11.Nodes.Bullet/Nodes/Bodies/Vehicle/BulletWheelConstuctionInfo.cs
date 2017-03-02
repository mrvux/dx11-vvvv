using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;
using VVVV.Internals.Bullet;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Bullet.DataTypes.Vehicle;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "WheelConstuctionInfo", Category = "Bullet", Version ="Create",
        Help = "", Author = "vux")]
    public unsafe class BulleteCreateWheelCstInfoNode : IPluginEvaluate
    {
        [Input("Local Position", DefaultValue = 0.7f)]
        protected ISpread<SlimDX.Vector3> LocalPosition;

        [Input("Connection height", DefaultValue = 1.2f)]
        protected ISpread<float> ConnectionHeight;

        [Input("Wheel Radius", DefaultValue =0.7f)]
        protected ISpread<float> WheelRadius;

        [Input("Wheels Width", DefaultValue =0.4f)]
        protected ISpread<float> WheelWidth;

        [Input("Suspension Rest Length", DefaultValue = 0.6f)]
        protected ISpread<float> SuspensionRestLength;

        [Input("Connection Height", DefaultValue = 1.2f)]
        protected ISpread<float> FrictionSlip;

        [Input("Is Front Wheel", DefaultValue = 1)]
        protected ISpread<bool> isFront;

        [Output("Output")]
        protected ISpread<WheelConstructionSettings> output;

        public void Evaluate(int SpreadMax)
        {
            this.output.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                this.output[i] = new WheelConstructionSettings()
                {
                    WheelRadius = WheelRadius[i],
                    ConnectionHeight = ConnectionHeight[i],
                    localPosition  = LocalPosition[i],
                    SuspensionRestLength = SuspensionRestLength[i],
                    WheelWidth = WheelWidth[i],
                    isFrontWheel = isFront[i]
                };
               
            }
        }
    }
}
