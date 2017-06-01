using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Nodes.Bullet;
using VVVV.Utils.VMath;

namespace VVVV.Bullet.Nodes.Bodies.Interactions.Vehicle
{
    [PluginInfo(Name = "Steer", Category = "Bullet", Version="Vehicle", Author = "vux",
        Help = "Drives Bullet Vehicle", AutoEvaluate = true)]
    public class BulletSteerVehicleNode : AbstractBodyInteractionNode<RaycastVehicle>
    {
        [Input("Steering", Order =10)]
        protected ISpread<float> FSteer;

        [Input("Wheel Index", Order = 11)]
        protected ISpread<int> FSteerWheel;

        protected override void ProcessObject(RaycastVehicle obj, int slice)
        {
            obj.SetSteeringValue(this.FSteer[slice], VMath.Zmod(this.FSteerWheel[slice], obj.NumWheels));
        }
    }

    [PluginInfo(Name = "Brake", Category = "Bullet", Version = "Vehicle", Author = "vux",
    Help = "Drives Bullet Vehicle", AutoEvaluate = true)]
    public class BulletBrakeVehicleNode : AbstractBodyInteractionNode<RaycastVehicle>
    {
        [Input("Brake Force", Order = 10)]
        protected ISpread<float> FBrakeForce;

        [Input("Wheel Index", Order = 11)]
        protected ISpread<int> FBrakeForceWheel;

        protected override void ProcessObject(RaycastVehicle obj, int slice)
        {
            obj.SetBrake(this.FBrakeForce[slice], VMath.Zmod(this.FBrakeForceWheel[slice], obj.NumWheels));
        }
    }

    [PluginInfo(Name = "EngineForce", Category = "Bullet", Version = "Vehicle", Author = "vux",
    Help = "Drives Bullet Vehicle", AutoEvaluate = true)]
    public class BulletEngineForceVehicleNode : AbstractBodyInteractionNode<RaycastVehicle>
    {
        [Input("Engine Force", Order = 10)]
        protected ISpread<float> FEngineForce;

        [Input("Wheel Index", Order = 11)]
        protected ISpread<int> FEngineForceWheel;

        protected override void ProcessObject(RaycastVehicle obj, int slice)
        {
            obj.ApplyEngineForce(this.FEngineForce[slice], VMath.Zmod(this.FEngineForceWheel[slice], obj.NumWheels));
        }
    }

    [PluginInfo(Name = "Info", Category = "Bullet", Version = "Vehicle", Author = "vux",
Help = "Info about bullet vehicle", AutoEvaluate = true)]
    public class BulletGetVehicleData : IPluginEvaluate
    {
        [Input("Input", Order = 10)]
        protected Pin<RaycastVehicle> input;

        [Output("Speed")]
        protected ISpread<float> speed;

        public void Evaluate(int SpreadMax)
        {
            if (this.input.IsConnected)
            {
                this.speed.SliceCount = input.SliceCount;
                for (int i = 0; i < SpreadMax; i++)
                {
                    var v = this.input[i];
                    this.speed[i] = v.CurrentSpeedKmHour;
                }
            }
            else
            {
                this.speed.SliceCount = 0;
            }
 
        }
    }
}
