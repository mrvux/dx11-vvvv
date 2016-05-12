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
    public struct SpotLight
    {
        public Vector3 Position;
        public float AttenuationStart;
        public Vector3 Direction;
        public float AttenuationEnd;
        public Vector3 Color;
        public float Fov;
        public float FovDecay;
    };

    [PluginInfo(Name = "DynamicBuffer", Category = "DX11", Version = "SpotLight", Author = "vux")]
    public class SpotLightBuffer : DynamicArrayBuffer<SpotLight>
    {
        [Input("View", AutoValidate = false)]
        protected Pin<Matrix> FView;

        [Input("Position", AutoValidate = false)]
        protected ISpread<Vector3> FPosition;

        [Input("Attenuation Start", AutoValidate = false, DefaultValue=1)]
        protected ISpread<float> FAttenStart;

        [Input("Direction", AutoValidate = false)]
        protected ISpread<Vector3> FDirection;

        [Input("Attenuation End", AutoValidate = false, DefaultValue =10)]
        protected ISpread<float> FAttenEnd;

        [Input("Color", AutoValidate = false)]
        protected ISpread<Vector3> FColor;

        [Input("Fov", AutoValidate = false, DefaultValue=0.5)]
        protected ISpread<float> FFov;

        [Input("Decay", AutoValidate = false, DefaultValue = 20)]
        protected ISpread<float> FDecay;

        protected override void BuildBuffer(int count, SpotLight[] buffer)
        {
            this.FView.Sync();
            this.FPosition.Sync();
            this.FAttenStart.Sync();
            this.FDirection.Sync();
            this.FColor.Sync();
            this.FFov.Sync();
            this.FAttenEnd.Sync();
            this.FDecay.Sync();

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


                if (this.FView.PluginIO.IsConnected)
                {
                    buffer[i].Direction = Vector3.Normalize(Vector3.TransformNormal(this.FDirection[i], this.FView[0]));
                }
                else
                {
                    buffer[i].Direction = Vector3.Normalize(this.FDirection[i]);
                }
                buffer[i].Color = this.FColor[i];
                buffer[i].AttenuationEnd = this.FAttenEnd[i];
                buffer[i].Fov = this.FFov[i];
                buffer[i].FovDecay = this.FDecay[i];
            }
        }
    }
}
