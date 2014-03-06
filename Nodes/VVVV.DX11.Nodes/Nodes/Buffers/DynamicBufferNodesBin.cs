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
    [PluginInfo(Name = "DynamicBufferBin", Category = "DX11.Buffer", Version = "Value", Author = "vux, microdee")]
    public class DynamicFloatStructBufferBin : DynamicStructBufferBin<float> { }

    [PluginInfo(Name = "DynamicBufferBin", Category = "DX11.Buffer", Version = "2d", Author = "vux, microdee")]
    public class DynamicVector2StructBufferBin : DynamicStructBufferBin<Vector2> { }

    [PluginInfo(Name = "DynamicBufferBin", Category = "DX11.Buffer", Version = "3d", Author = "vux, microdee")]
    public class DynamicVector3StructBufferBin : DynamicStructBufferBin<Vector3> { }

    [PluginInfo(Name = "DynamicBufferBin", Category = "DX11.Buffer", Version = "4d", Author = "vux, microdee")]
    public class DynamicVector4StructBufferBin : DynamicStructBufferBin<Vector4> { }

    [PluginInfo(Name = "DynamicBufferBin", Category = "DX11.Buffer", Version = "Transform", Author = "vux, microdee")]
    public class DynamicMatrixBufferBin : DynamicStructBufferBin<Matrix>
    {
        [Input("Transpose", DefaultValue = 1,Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<bool> FTranspose;


        protected override void WriteArray(int count, int i)
        {
            if (!this.FTranspose[0])
            {
                base.WriteArray(count, i);
            }
            else
            {
                for (int j = 0; j < count; j++)
                {
                    this.tempbuffer[j] = Matrix.Transpose(this.FInData[i][j]);
                }
            }
        }
    }

    [PluginInfo(Name = "DynamicBufferBin", Category = "DX11.Buffer", Version = "Color", Author = "vux, microdee")]
    public class DynamicColor4BufferBin : DynamicStructBufferBin<Color4> { }

    [PluginInfo(Name = "DynamicBufferBin", Category = "DX11.Buffer", Version = "Int", Author = "vux, microdee")]
    public class DynamicIntBufferBin : DynamicStructBufferBin<int> { }

    [PluginInfo(Name = "DynamicBufferBin", Category = "DX11.Buffer", Version = "UInt", Author = "vux, microdee")]
    public class DynamicUIntBufferBin : DynamicStructBufferBin<uint> { }
}
