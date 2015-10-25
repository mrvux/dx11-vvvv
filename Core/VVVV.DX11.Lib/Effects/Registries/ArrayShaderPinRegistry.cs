using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;

namespace VVVV.DX11.Lib.Effects.Registries
{
    public class ArrayShaderPinRegistry : ShaderPinRegistry
    {
        public ArrayShaderPinRegistry()
        {
            this.RegisterType("bool", (var) => new BoolArrayShaderPin());
            this.RegisterType("float", (var) => new FloatArrayShaderPin());
            this.RegisterType("int", (var) => new IntArrayShaderPin());
            this.RegisterType("float2", (var) => new Float2ArrayShaderPin());
            this.RegisterType("float3", (var) => new Float3ArrayShaderPin());
            this.RegisterType("float4", (var) => { if (var.IsColor()) { return new ColorArrayShaderPin(); } else { return new Float4ArrayShaderPin(); } });
           
            
            
            this.RegisterType("float4x4", (var) => new MatrixArrayShaderPin());

            this.RegisterType("Texture2D", (var) => new Texture2DArrayShaderPin());
        }

    }
}
