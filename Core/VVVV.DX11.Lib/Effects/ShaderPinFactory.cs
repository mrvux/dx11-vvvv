using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.DX11.Lib.Effects;
using VVVV.DX11.Lib.Effects.Registries;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Internals.Effects
{
    

    public static class ShaderPinFactory
    {
        private static StandardShaderPinRegistry stdregistry = new StandardShaderPinRegistry();
        private static ArrayShaderPinRegistry arrayregistry = new ArrayShaderPinRegistry();

        private static RenderVariableRegistry renderregistry = new RenderVariableRegistry();
        private static WorldRenderVariableRegistry worldregistry = new WorldRenderVariableRegistry();

        public static bool IsRenderVariable(EffectVariable var)
        {
            string semantic = var.Description.Semantic;
            string type = var.GetVariableType().Description.TypeName;
            bool array = var.GetVariableType().Description.Elements > 0;

            return renderregistry.ContainsType(type, semantic, array);

        }

        public static bool IsWorldRenderVariable(EffectVariable var)
        {
            string semantic = var.Description.Semantic;
            string type = var.GetVariableType().Description.TypeName;
            bool array = var.GetVariableType().Description.Elements > 0;

            return worldregistry.ContainsType(type, semantic, array);
        }

        public static bool IsShaderPin(EffectVariable var)
        {
            string semantic = var.Description.Semantic;
            string type = var.GetVariableType().Description.TypeName;
            bool array = var.GetVariableType().Description.Elements > 0;

            return ((stdregistry.ContainsType(type)
                || arrayregistry.ContainsType(type)) && semantic == "");
                //|| semanticregistry.ContainsType(type, semantic, array)) && (semantic != "IMMUTABLE");
        }

        public static IRenderVariable GetRenderVariable(EffectVariable var, IPluginHost host, IIOFactory iofactory)
        {
            return renderregistry.CreatePin(var.GetVariableType().Description.TypeName, var.Description.Semantic, var.GetVariableType().Description.Elements > 0, var, host, iofactory);
        }

        public static IWorldRenderVariable GetWorldRenderVariable(EffectVariable var, IPluginHost host, IIOFactory iofactory)
        {
            return worldregistry.CreatePin(var.GetVariableType().Description.TypeName, var.Description.Semantic, var.GetVariableType().Description.Elements > 0, var, host, iofactory);
        }

        public static IShaderPin GetShaderPin(EffectVariable var, IPluginHost host, IIOFactory iofactory)
        {
            string semantic = var.Description.Semantic;
            string type = var.GetVariableType().Description.TypeName;
            bool array = var.GetVariableType().Description.Elements > 0;
            //Exclude if immutable
            if (semantic != "") { return null; }

            if (array)
            {
                return arrayregistry.CreatePin(type, var, host, iofactory);
            }
            else
            {
                return stdregistry.CreatePin(type, var, host, iofactory);
            }

        }
    }
}
