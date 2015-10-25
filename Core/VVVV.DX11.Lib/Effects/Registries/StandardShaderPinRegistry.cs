using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.DX11.Lib.Effects.Pins.Resources;


namespace VVVV.DX11.Lib.Effects.Registries
{

    public class StandardShaderPinRegistry : ShaderPinRegistry
    {
        public StandardShaderPinRegistry() 
        {
            //Basic scalar/vector types 
            this.RegisterType("int", (var) => new IntShaderPin());
            this.RegisterType("int2", (var) => new Int2ShaderPin());
            this.RegisterType("int3", (var) => new Int3ShaderPin());
            this.RegisterType("int4", (var) => new Int4ShaderPin());

            this.RegisterType("uint", (var) => new IntShaderPin());
            this.RegisterType("uint2", (var) => new Int2ShaderPin());
            this.RegisterType("uint3", (var) => new Int3ShaderPin());
            this.RegisterType("uint4", (var) => new Int4ShaderPin());

            this.RegisterType("bool", (var) => new BoolShaderPin());

            this.RegisterType("float", (var) => new FloatShaderPin());
            this.RegisterType("float2", (var) => new Float2ShaderPin());
            this.RegisterType("float3", (var) => new Float3ShaderPin());
            this.RegisterType("float4", (var) => { if (var.IsColor()) { return new ColorShaderPin(); } else { return new Float4ShaderPin(); } });
           
            
            this.RegisterType("float4x4", (var) => new MatrixShaderPin());

            
            //Textures
            this.RegisterType("Texture1D", (var) => new Texture1DShaderPin());
            this.RegisterType("Texture1DArray", (var) => new Texture1DShaderPin());
            this.RegisterType("Texture2D", (var) => new Texture2DShaderPin());
            this.RegisterType("Texture3D", (var) => new Texture3DShaderPin());
            this.RegisterType("Texture2DMS", (var) => new Texture2DShaderPin());
            this.RegisterType("Texture2DMSArray", (var) => new Texture2DShaderPin());
            this.RegisterType("Texture2DArray", (var) => new Texture2DShaderPin());
            this.RegisterType("TextureCube", (var) => new TextureCubeShaderPin());
            this.RegisterType("TextureCubeArray", (var) => new TextureCubeShaderPin());
            
            //Sampler
            this.RegisterType("SamplerState", (var) => new SamplerShaderPin());
            this.RegisterType("SamplerComparisonState", (var) => new SamplerShaderPin()); 

            //Buffers
            this.RegisterType("StructuredBuffer", (var) => new ReadableStructuredBufferShaderPin());
            this.RegisterType("Buffer", (var) => new ReadableBufferShaderPin());
            this.RegisterType("ByteAddressBuffer", (var) => new ReadableBufferShaderPin());
        }
    }
}
