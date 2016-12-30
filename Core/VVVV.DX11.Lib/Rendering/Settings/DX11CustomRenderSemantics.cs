using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

using FeralTic.Resources;
using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Lib.Rendering
{
    public class IntRenderSemantic : DX11RenderSemantic<int>
    {
        public IntRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "int" }; }
        public IntRenderSemantic(string semantic, bool mandatory, int data) : this(semantic, mandatory) { this.Data = data; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data); }
    }

    public class FloatRenderSemantic : DX11RenderSemantic<float>
    {
        public FloatRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "float" }; }
        public FloatRenderSemantic(string semantic, bool mandatory, float data) : this(semantic, mandatory) { this.Data = data; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data); }
    }

    public class Vector2RenderSemantic : DX11RenderSemantic<Vector2>
    {
        public Vector2RenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "float2" }; }
        public Vector2RenderSemantic(string semantic, bool mandatory, Vector2 data) : this(semantic, mandatory) { this.Data = data; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data); }
    }

    public class Vector3RenderSemantic : DX11RenderSemantic<Vector3>
    {
        public Vector3RenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "float3" }; }
        public Vector3RenderSemantic(string semantic, bool mandatory, Vector3 data) : this(semantic, mandatory) { this.Data = data; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data); }
    }

    public class Vector4RenderSemantic : DX11RenderSemantic<Vector4>
    {
        public Vector4RenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "float4" }; }
        public Vector4RenderSemantic(string semantic, bool mandatory, Vector4 data) : this(semantic, mandatory) { this.Data = data; }
        protected override void ApplyVariable(string name,DX11ShaderInstance instance) { instance.SetByName(name,this.Data);}
    }

    public class MatrixRenderSemantic : DX11RenderSemantic<Matrix>
    {
        public MatrixRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "float4x4" }; }
        public MatrixRenderSemantic(string semantic, bool mandatory, Matrix data) : this(semantic, mandatory) { this.Data = data; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data); }
    }

    public class Texture1dRenderSemantic : DX11RenderSemantic<DX11Texture1D>
    {
        public Texture1dRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "Texture1D" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.SRV : null); }
    }

    public class Texture2dRenderSemantic : DX11RenderSemantic<DX11Texture2D>
    {
        public Texture2dRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "Texture2D", "Texture2DMS", "Texture2DArray", "Texture2DMSArray" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.SRV : null); }
    }

    public class Texture2dArrayRenderSemantic : DX11RenderSemantic<DX11Texture2D>
    {
        public Texture2dArrayRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "Texture2DArray", "Texture2DMSArray" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.SRV : null); }
    }

    public class TextureCubeRenderSemantic : DX11RenderSemantic<DX11Texture2D>
    {
        public TextureCubeRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "TextureCube" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.SRV : null); }
    }

    public class RWTexture2dRenderSemantic : DX11RenderSemantic<IDX11RWResource>
    {
        public RWTexture2dRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "RWTexture2D" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.UAV : null); }
    }

    public class Texture3dRenderSemantic : DX11RenderSemantic<DX11Texture3D>
    {
        public Texture3dRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "Texture3D" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.SRV : null); }
    }

    public class RWTexture3dRenderSemantic : DX11RenderSemantic<IDX11RWResource>
    {
        public RWTexture3dRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "RWTexture3D" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.UAV : null); }
    }

    public class RWStructuredBufferRenderSemantic : DX11RenderSemantic<IDX11RWResource>
    {
        public RWStructuredBufferRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "RWStructuredBuffer" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.UAV : null); }
    }

    public class StructuredBufferRenderSemantic : DX11RenderSemantic<IDX11ReadableResource>
    {
        public StructuredBufferRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "StructuredBuffer" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.SRV : null); }
    }

    public class RWBufferRenderSemantic : DX11RenderSemantic<IDX11RWResource>
    {
        public RWBufferRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "RWByteAddressBuffer" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.UAV : null); }
    }

    public class BufferRenderSemantic : DX11RenderSemantic<IDX11ReadableResource>
    {
        public BufferRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "ByteAddressBuffer" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) { instance.SetByName(name, this.Data != null ? this.Data.SRV : null); }
    }

    //

    public class AppendStructuredBufferRenderSemantic : DX11RenderSemantic<IDX11RWResource>
    {
        public int Counter { get; set; }
        public bool ResetCounter { get; set; }
        public AppendStructuredBufferRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "AppendStructuredBuffer" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) 
        {
            if (this.ResetCounter)
            {
                instance.SetByName(name, this.Data.UAV, this.Counter);
            }
            else
            {
                instance.SetByName(name, this.Data.UAV);
            }
        }
    }

    public class ConsumeStructuredBufferRenderSemantic : DX11RenderSemantic<IDX11RWResource>
    {
        public int Counter { get; set; }
        public bool ResetCounter { get; set; }
        public ConsumeStructuredBufferRenderSemantic(string semantic, bool mandatory) : base(semantic, mandatory) { TypeNames = new string[] { "ConsumeStructuredBuffer" }; }
        protected override void ApplyVariable(string name, DX11ShaderInstance instance) 
        {
            if (this.ResetCounter)
            {
                instance.SetByName(name, this.Data.UAV, this.Counter);
            }
            else
            {
                instance.SetByName(name, this.Data.UAV);
            }
        }
    }

    
    
}
