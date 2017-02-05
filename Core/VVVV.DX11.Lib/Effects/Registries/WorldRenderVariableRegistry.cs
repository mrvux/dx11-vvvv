using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;
using VVVV.DX11.Lib.Effects.Pins.RenderSemantics;

namespace VVVV.DX11.Lib.Effects.Registries
{
    public class WorldRenderVariableRegistry : CustomVariableRegistry<IWorldRenderVariable>
    {
        public WorldRenderVariableRegistry()
        {
            //View projection semantics
            this.RegisterType("float4x4", "WORLD", false, (var, host,factory) => new MatrixWorldRenderVariable(var));
            this.RegisterType("float4x4", "WORLDTRANSPOSE", false, (var, host, factory) => new MatrixWorldTransposeRenderVariable(var));
            this.RegisterType("float4x4", "WORLDINVERSE", false, (var, host, factory) => new MatrixWorldInvRenderVariable(var));
            this.RegisterType("float4x4", "WORLDINVERSETRANSPOSE", false, (var, host, factory) => new MatrixWorldInverseTransposeRenderVariable(var));

            this.RegisterType("float4x4", "WORLDLAYER", false, (var, host, factory) => new MatrixWorldLayerRenderVariable(var));
            this.RegisterType("float4x4", "WORLDLAYERINVERSETRANSPOSE", false, (var, host, factory) => new MatrixWorldLayerInverseTransposeRenderVariable(var));

            this.RegisterType("float4x4", "WORLDVIEW", false, (var, host, factory) => new MatrixWorldViewRenderVariable(var));
            this.RegisterType("float4x4", "WORLDLAYERVIEW", false, (var, host, factory) => new MatrixWorldLayerViewRenderVariable(var));

            this.RegisterType("float4x4", "WORLDVIEWPROJECTION", false, (var, host, factory) => new MatrixWorldViewProjRenderVariable(var));
            this.RegisterType("float4x4", "WORLDLAYERVIEWPROJECTION", false, (var, host, factory) => new MatrixWorldLayerViewProjectionRenderVariable(var));

            this.RegisterType("int", "DRAWINDEX", false, (var, host, factory) => new IntDrawIndexRenderVariable(var));
            this.RegisterType("float", "DRAWINDEX", false, (var, host, factory) => new FloatDrawIndexRenderVariable(var));

            this.RegisterType("int", "ITERATIONINDEX", false, (var, host, factory) => new IterIndexRenderVariable(var));
            this.RegisterType("int", "ITERATIONCOUNT", false, (var, host, factory) => new IterCountRenderVariable(var));
            
            this.RegisterType("float3", "BOUNDINGMIN", false, (var, host, factory) => new ObjectBMinRenderVariable(var));
            this.RegisterType("float3", "BOUNDINGMAX", false, (var, host, factory) => new ObjectBMinRenderVariable(var));
            this.RegisterType("float3", "BOUNDINGSCALE", false, (var, host, factory) => new ObjectBMinRenderVariable(var));

            this.RegisterType("float4x4", "OBJUNITTRANS", false, (var, host, factory) => new ObjectUnitTransformRenderVariable(var));
            this.RegisterType("float4x4", "OBJSDFTRANS", false, (var, host, factory) => new ObjectSdfTransformRenderVariable(var));
        }
    }
}
