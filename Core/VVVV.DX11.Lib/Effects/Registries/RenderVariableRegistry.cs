using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;
using VVVV.DX11.Lib.Effects.Pins.RenderSemantics;
//using VVVV.DX11.Lib.Effects.Pins.RenderSemantics;

namespace VVVV.DX11.Lib.Effects.Registries
{
    public class RenderVariableRegistry : CustomVariableRegistry<IRenderVariable>
    {
        public RenderVariableRegistry()
        {
            //View projection semantics
            this.RegisterType("float4x4", "LAYER", false, (var, host, factory) => new MatrixLayerWorldRenderVariable(var));
            this.RegisterType("float4x4", "LAYERINVERSE", false, (var, host, factory) => new MatrixLayerInvWorldRenderVariable(var));

            this.RegisterType("float4x4", "LAYERVIEW", false, (var, host, factory) => new MatrixLayerWorldViewRenderVariable(var));
            this.RegisterType("float4x4", "LAYERVIEWPROJECTION", false, (var, host, factory) => new MatrixLayerWorldViewProjRenderVariable(var));

            this.RegisterType("float4x4", "PROJECTION", false, (var, host, factory) => new MatrixProjRenderVariable(var));
            this.RegisterType("float4x4", "VIEW", false, (var, host, factory) => new MatrixViewRenderVariable(var));
            this.RegisterType("float4x4", "VIEWPROJECTION", false, (var, host, factory) => new MatrixViewProjRenderVariable(var));

            this.RegisterType("float4x4", "PROJECTIONINVERSE", false, (var, host, factory) => new MatrixInvProjRenderVariable(var));
            this.RegisterType("float4x4", "VIEWINVERSE", false, (var, host, factory) => new MatrixInvViewRenderVariable(var));
            this.RegisterType("float4x4", "VIEWPROJECTIONINVERSE", false, (var, host, factory) => new MatrixInvViewProjRenderVariable(var));

            this.RegisterType("float4x4", "PROJECTIONTRANSPOSE", false, (var, host, factory) => new MatrixProjTransposeRenderVariable(var));
            this.RegisterType("float4x4", "VIEWTRANSPOSE", false, (var, host, factory) => new MatrixViewTransposeRenderVariable(var));
            this.RegisterType("float4x4", "VIEWPROJECTIONTRANSPOSE", false, (var, host, factory) => new MatrixViewProjTransposeRenderVariable(var));

            this.RegisterType("float4x4", "PROJECTIONINVERSETRANSPOSE", false, (var, host, factory) => new MatrixInvProjTransposeRenderVariable(var));
            this.RegisterType("float4x4", "VIEWINVERSETRANSPOSE", false, (var, host, factory) => new MatrixInvViewTransposeRenderVariable(var));
            this.RegisterType("float4x4", "VIEWPROJECTIONINVERSETRANSPOSE", false, (var, host, factory) => new MatrixInvViewProjTransposeRenderVariable(var));

            this.RegisterType("float3", "CAMERAPOSITION", false, (var, host, factory) => new CameraPositionRenderVariable(var));


            this.RegisterType("RWTexture1D", "BACKBUFFER", false, (var, host, factory) => new RWBackBufferRenderVariable(var));
            this.RegisterType("RWTexture1DArray", "BACKBUFFER", false, (var, host, factory) => new RWBackBufferRenderVariable(var));
            this.RegisterType("RWTexture2D", "BACKBUFFER", false, (var, host, factory) => new RWBackBufferRenderVariable(var));
            this.RegisterType("RWTexture3D", "BACKBUFFER", false, (var, host, factory) => new RWBackBufferRenderVariable(var));
            this.RegisterType("RWStructuredBuffer", "BACKBUFFER", false, (var, host, factory) => new RWBackBufferRenderVariable(var));

            this.RegisterType("Texture2D", "READBUFFER", false, (var, host, factory) => new ReadBufferRenderVariable(var));
            this.RegisterType("Texture3D", "READBUFFER", false, (var, host, factory) => new ReadBufferRenderVariable(var));
            this.RegisterType("StructuredBuffer", "READBUFFER", false, (var, host, factory) => new ReadBufferRenderVariable(var));
            
            this.RegisterType("Texture2D", "READONLYDEPTHTEXTURE", false, (var, host, factory) => new ReadOnlyDepthRenderVariable(var));
            this.RegisterType("Texture2DMS", "READONLYDEPTHTEXTURE", false, (var, host, factory) => new ReadOnlyDepthRenderVariable(var));

            this.RegisterType("float2", "TARGETSIZE", false, (var, host, factory) => new Float2TargetSizeRenderVariable(var));
            this.RegisterType("float2", "INVTARGETSIZE", false, (var, host, factory) => new Float2InvTargetSizeRenderVariable(var));
            this.RegisterType("float4", "TARGETSIZE", false, (var, host, factory) => new Float4TargetSizeRenderVariable(var));

            this.RegisterType("float3", "TARGETSIZE", false, (var, host, factory) => new Float3TargetSizeRenderVariable(var));
            this.RegisterType("float3", "INVTARGETSIZE", false, (var, host, factory) => new Float3InvTargetSizeRenderVariable(var));

            this.RegisterType("float4", "VOLUMESIZE", false, (var, host, factory) => new Float4VolumeSizeRenderVariable(var));

            this.RegisterType("int", "DRAWCOUNT", false, (var, host, factory) => new IntDrawCountRenderVariable(var));
            this.RegisterType("float", "DRAWCOUNT", false, (var, host, factory) => new FloatDrawCountRenderVariable(var));
            this.RegisterType("float", "INVDRAWCOUNT", false, (var, host, factory) => new InvFloatDrawCountRenderVariable(var));

            this.RegisterType("int", "VIEWPORTCOUNT", false, (var, host, factory) => new IntViewPortCountRenderVariable(var));
            this.RegisterType("int", "VIEWPORTINDEX", false, (var, host, factory) => new IntViewPortIndexRenderVariable(var));

            this.RegisterType("int", "ELEMENTCOUNT", false, (var, host, factory) => new IntElemSizeRenderVariable(var));
            this.RegisterType("RWStructuredBuffer", "BACKBUFFER", false, (var, host, factory) => new RWBackBufferRenderVariable(var));
            this.RegisterType("AppendStructuredBuffer", "BACKBUFFER", false, (var, host, factory) => new RWBackBufferRenderVariable(var));
            this.RegisterType("ConsumeStructuredBuffer", "BACKBUFFER", false, (var, host, factory) => new RWBackBufferRenderVariable(var));
            this.RegisterType("RWByteAddressBuffer", "BACKBUFFER", false, (var, host, factory) => new RWBackBufferRenderVariable(var));
        }
    }
}
