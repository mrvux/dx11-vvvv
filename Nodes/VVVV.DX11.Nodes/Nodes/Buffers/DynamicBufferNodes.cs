using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;



namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "DynamicBuffer", Category = "DX11.Buffer", Version = "Value", Author = "vux")]
    public class DynamicFloatStructBuffer : DynamicStructBuffer<float> {}

    [PluginInfo(Name = "DynamicBuffer", Category = "DX11.Buffer", Version = "2d", Author = "vux")]
    public class DynamicVector2StructBuffer : DynamicStructBuffer<Vector2> {}

    [PluginInfo(Name = "DynamicBuffer", Category = "DX11.Buffer", Version = "3d", Author = "vux")]
    public class DynamicVector3StructBuffer : DynamicStructBuffer<Vector3> { }

    [PluginInfo(Name = "DynamicBuffer", Category = "DX11.Buffer", Version = "4d", Author = "vux")]
    public class DynamicVector4StructBuffer : DynamicStructBuffer<Vector4> { }

    [PluginInfo(Name = "DynamicBuffer", Category = "DX11.Buffer", Version = "Transform", Author = "vux")]
    public class DynamicMatrixBuffer : DynamicStructBuffer<Matrix>
    {
        [Input("Transpose", DefaultValue = 1,Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<bool> FTranspose;


        protected override void WriteArray(int count)
        {
            if (!this.FTranspose[0])
            {
                base.WriteArray(count);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    this.tempbuffer[i] = Matrix.Transpose(this.FInData[i]);
                }
            }
        }
    }

    [PluginInfo(Name = "DynamicBuffer", Category = "DX11.Buffer", Version = "Color", Author = "vux")]
    public class DynamicColor4Buffer : DynamicStructBuffer<Color4> { }

    [PluginInfo(Name = "DynamicBuffer", Category = "DX11.Buffer", Version = "Int", Author = "vux")]
    public class DynamicIntBuffer : DynamicStructBuffer<int> { }

    [PluginInfo(Name = "DynamicBuffer", Category = "DX11.Buffer", Version = "UInt", Author = "vux")]
    public class DynamicUIntBuffer : DynamicStructBuffer<uint> { }
}
