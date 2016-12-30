using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.DX11.Nodes;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DX11
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointLight
    {
        public Vector3 Position;
        public float AttenuationStart;
        public Vector3 Color;
        public float AttenuationEnd;
    }

    [PluginInfo(Name = "DynamicBuffer", Category = "DX11", Version = "PointLight", Author = "vux")]
    public class PointLightBuffer : DynamicArrayBuffer<PointLight>
    {
        [Input("View", AutoValidate = false)]
        protected Pin<Matrix> FView;

        [Input("Position", AutoValidate = false)]
        protected ISpread<Vector3> FPosition;

        [Input("Attenuation Start", AutoValidate = false,DefaultValue=1)]
        protected ISpread<float> FAttenStart;

        [Input("Color", AutoValidate = false)]
        protected ISpread<Vector3> FColor;

        [Input("Attenuation End", AutoValidate = false, DefaultValue = 10)]
        protected ISpread<float> FAttenEnd;

        protected override void BuildBuffer(int count, PointLight[] buffer)
        {
            this.FView.Sync();
            this.FPosition.Sync();
            this.FColor.Sync();
            this.FAttenStart.Sync();
            this.FAttenEnd.Sync();

            for (int i = 0; i < count; i++)
            {
                if (this.FView.PluginIO.IsConnected)
                {
                    buffer[i].Position = Vector3.TransformCoordinate(this.FPosition[i], this.FView[0]);
                }
                else
                {
                    buffer[i].Position = this.FPosition[i];
                }

                buffer[i].AttenuationStart = this.FAttenStart[i];
                buffer[i].Color = this.FColor[i];
                buffer[i].AttenuationEnd = this.FAttenEnd[i];
            }
        }
    }
}
