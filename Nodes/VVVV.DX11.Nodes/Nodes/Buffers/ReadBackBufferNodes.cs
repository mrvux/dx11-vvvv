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
    [PluginInfo(Name = "ReadBack", Category = "DX11.Buffer", Version = "Value", Author = "vux")]
    public class ReadBackFloatStructBuffer : ReadBackBufferBaseNode<float> 
    {
        protected override void WriteData(DataStream ds,int elementcount)
        {
            ds.ReadRange<float>(this.FOutput.Stream.Buffer, 0, elementcount);
        }
    }

    [PluginInfo(Name = "ReadBack", Category = "DX11.Buffer", Version = "2d", Author = "vux")]
    public class ReadBackVector2StructBuffer : ReadBackBufferBaseNode<Vector2>
    {
        protected override void WriteData(DataStream ds, int elementcount)
        {
            ds.ReadRange<Vector2>(this.FOutput.Stream.Buffer, 0, elementcount);
        }
    }

    [PluginInfo(Name = "ReadBack", Category = "DX11.Buffer", Version = "3d", Author = "vux")]
    public class ReadBackVector3StructBuffer : ReadBackBufferBaseNode<Vector3>
    {
        protected override void WriteData(DataStream ds, int elementcount)
        {
            ds.ReadRange<Vector3>(this.FOutput.Stream.Buffer, 0, elementcount);
        }
    }


    [PluginInfo(Name = "ReadBack", Category = "DX11.Buffer", Version = "4d", Author = "vux")]
    public class ReadBackVector4StructBuffer : ReadBackBufferBaseNode<Vector4>
    {
        protected override void WriteData(DataStream ds, int elementcount)
        {
            ds.ReadRange<Vector4>(this.FOutput.Stream.Buffer, 0, elementcount);
        }
    }

    [PluginInfo(Name = "ReadBack", Category = "DX11.Buffer", Version = "Color", Author = "vux")]
    public class ReadBackColor4StructBuffer : ReadBackBufferBaseNode<Color4>
    {
        protected override void WriteData(DataStream ds, int elementcount)
        {
            ds.ReadRange<Color4>(this.FOutput.Stream.Buffer, 0, elementcount);
        }
    }

    [PluginInfo(Name = "ReadBack", Category = "DX11.Buffer", Version = "Int", Author = "vux")]
    public class ReadBackIntStructBuffer : ReadBackBufferBaseNode<int>
    {
        protected override void WriteData(DataStream ds, int elementcount)
        {
            ds.ReadRange<int>(this.FOutput.Stream.Buffer, 0, elementcount);
        }
    }

    [PluginInfo(Name = "ReadBack", Category = "DX11.Buffer", Version = "UInt", Author = "vux")]
    public class ReadBackUIntStructBuffer : ReadBackBufferBaseNode<uint>
    {
        protected override void WriteData(DataStream ds, int elementcount)
        {
            ds.ReadRange<uint>(this.FOutput.Stream.Buffer, 0, elementcount);
        }
    }

    [PluginInfo(Name = "ReadBack", Category = "DX11.Buffer", Version = "Transform", Author = "vux")]
    public class ReadBackMatrixStructBuffer : ReadBackBufferBaseNode<Matrix>
    {
        [Input("Transpose", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<bool> FTranspose;


        protected override void WriteData(DataStream ds, int elementcount)
        {
            Matrix[] buffer = this.FOutput.Stream.Buffer;
            ds.ReadRange<Matrix>(buffer, 0, elementcount);

            if (this.FTranspose[0])
            {
                for (int i = 0; i < this.FOutput.SliceCount;i++)
                {
                    buffer[i] = Matrix.Transpose(buffer[i]);
                }
            }
        }
    }

}
