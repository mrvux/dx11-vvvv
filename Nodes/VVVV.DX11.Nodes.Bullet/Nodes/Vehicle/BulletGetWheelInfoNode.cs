using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;

namespace VVVV.Bullet.Nodes.Vehicle
{
    [PluginInfo(Name = "WheelInfo", Category = "Bullet", Version = "Vehicle", Author = "vux",
    Help = "Gets vehicle wheel information", AutoEvaluate = false)]
    public class BuletGetWheelInfomNode : IPluginEvaluate
    {
        [Input("Vehicle")]
        protected Pin<RaycastVehicle> FInVehicle;

        [Output("Skid Info")]
        protected ISpread<ISpread<float>> FOutSkidInfo;

        [Output("Suspension Relative Velocity")]
        protected ISpread<ISpread<float>> FOutSuspensionRelativeVelocity;

        [Output("Suspension Force")]
        protected ISpread<ISpread<float>> FOutSuspensionsForce;

        public void Evaluate(int SpreadMax)
        {
            if (FInVehicle.PluginIO.IsConnected)
            {
                FOutSkidInfo.SliceCount = this.FInVehicle.SliceCount;
                FOutSuspensionRelativeVelocity.SliceCount = this.FInVehicle.SliceCount;
                FOutSuspensionsForce.SliceCount = this.FInVehicle.SliceCount;

                for (int i = 0; i < this.FInVehicle.SliceCount;i++)
                {
                    int numWheels = this.FInVehicle[i].NumWheels;
                    this.FOutSkidInfo[i].SliceCount = numWheels;
                    FOutSuspensionRelativeVelocity[i].SliceCount = numWheels;
                    FOutSuspensionsForce[i].SliceCount = numWheels;

                    RaycastVehicle v = this.FInVehicle[i];
                    for (int j = 0; j < v.NumWheels; j++)
                    {
                        WheelInfo wi = v.GetWheelInfo(j);

                        this.FOutSkidInfo[i][j] = wi.SkidInfo;
                        this.FOutSuspensionRelativeVelocity[i][j] = wi.SuspensionRelativeVelocity;
                        this.FOutSuspensionsForce[i][j] = wi.WheelsSuspensionForce;
                    }
                }
                    

            }
            else
            {
                this.FOutSkidInfo.SliceCount = 0;
                FOutSuspensionRelativeVelocity.SliceCount = 0;
                FOutSuspensionsForce.SliceCount = 0;
            }
        }
    }
}
