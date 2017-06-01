using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;

namespace VVVV.Bullet.Nodes.Vehicle
{
    [PluginInfo(Name = "WheelTransform", Category = "Bullet", Version = "Vehicle", Author = "vux",
    Help = "Gets vehicle wheel trnasforms", AutoEvaluate = true)]
    public class BuletGetWheelTransformNode : IPluginEvaluate
    {
        [Input("Vehicle")]
        protected Pin<RaycastVehicle> FInVehicle;

        [Output("Transform")]
        protected ISpread<ISpread<Matrix4x4>> FOutTransform;

        public void Evaluate(int SpreadMax)
        {
            if (FInVehicle.PluginIO.IsConnected)
            {
                FOutTransform.SliceCount = this.FInVehicle.SliceCount;

                for (int i = 0; i < this.FInVehicle.SliceCount;i++)
                {
                    this.FOutTransform[i].SliceCount = this.FInVehicle[i].NumWheels;

                    RaycastVehicle v = this.FInVehicle[i];

                    for (int j = 0; j < v.NumWheels; j++)
                    {
                        WheelInfo wi = v.GetWheelInfo(j);

                        Matrix m = wi.WorldTransform;

                        Matrix4x4 mn = new Matrix4x4(m.M11, m.M12, m.M13, m.M14,
                                    m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34,
                                    m.M41, m.M42, m.M43, m.M44);
                        this.FOutTransform[i][j] = mn;
                    }
                }
                    

            }
            else
            {
                this.FOutTransform.SliceCount = 0;
            }
        }
    }
}
