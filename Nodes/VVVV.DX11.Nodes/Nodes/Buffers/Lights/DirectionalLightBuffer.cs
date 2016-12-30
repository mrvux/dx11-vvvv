using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11.Nodes;

using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;


namespace VVVV.Nodes.DX11
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionalLight
    {
        public Vector3 Direction;
        public Vector3 Color;
    }

    [PluginInfo(Name = "DynamicBuffer", Category = "DX11", Version = "DirectionalLight", Author = "vux")]
    public class DirectionalLightBuffer : DynamicArrayBuffer<DirectionalLight>
    {
        [Input("Direction", AutoValidate = false)]
        protected ISpread<Vector3> FDirection;

        [Input("Color", AutoValidate = false)]
        protected ISpread<Vector3> FColor;
        

        protected override void BuildBuffer(int count, DirectionalLight[] buffer)
        {
            this.FDirection.Sync();
            this.FColor.Sync();

            for (int i = 0; i < count; i++)
            {
                buffer[i].Direction = this.FDirection[i];
                buffer[i].Color = this.FColor[i];
            }
        }
    }
}
