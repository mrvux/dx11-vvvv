using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using VVVV.DX11.Internals.Effects.Pins;
using VVVV.DX11.Lib.Effects.Pins;
using FeralTic.Utils;
using FeralTic;

namespace VVVV.DX11
{
    public static class EffectVariableExtensions
    {
        public static string UiName(this EffectVariable var)
        {
            string name = var.Description.Name;
            if (var.GetAnnotationByName("uiname") != null)
            {
                if (var.GetAnnotationByName("uiname").AsString() != null)
                {
                    name = var.GetAnnotationByName("uiname").AsString().GetString();
                }
            }
            return name;
        }

        public static AspectRatioMode AspectMode(this EffectVariable var)
        {
            if (var.GetAnnotationByName("aspectmode") != null)
            {
                if (var.GetAnnotationByName("aspectmode").AsString() != null)
                {
                    string mode = var.GetAnnotationByName("aspectmode").AsString().GetString();

                    AspectRatioMode aspectMode;
                    if (Enum.TryParse<AspectRatioMode>(mode, out aspectMode))
                    {
                        return aspectMode;
                    }
                    else
                    {
                        return AspectRatioMode.FitIn;
                    }
                }
            }
            return AspectRatioMode.FitIn;
        }

        public static bool Reference(this EffectVariable var, string variableName)
        {
            if (var.GetAnnotationByName("ref") != null)
            {
                if (var.GetAnnotationByName("ref").AsString() != null)
                {
                    string name = var.GetAnnotationByName("ref").AsString().GetString();
                    return name == variableName;
                }
            }
            return false;
        }

        public static bool Visible(this EffectVariable var)
        {
            bool res = true;
            if (var.GetAnnotationByName("visible") != null)
            {
                if (var.GetAnnotationByName("visible").AsScalar() != null)
                {
                    res = var.GetAnnotationByName("visible").AsScalar().GetFloat() > 0.5f;
                }
            }
            return res;
        }

        public static double UiMin(this EffectVariable var)
        {
            double res = double.MinValue;
            if (var.GetAnnotationByName("uimin") != null)
            {
                if (var.GetAnnotationByName("uimin").AsScalar() != null)
                {
                    res = var.GetAnnotationByName("uimin").AsScalar().GetFloat();
                }
            }
            return res;
        }

        public static double UiMax(this EffectVariable var)
        {
            double res = double.MaxValue;
            if (var.GetAnnotationByName("uimax") != null)
            {
                if (var.GetAnnotationByName("uimax").AsScalar() != null)
                {
                    res = var.GetAnnotationByName("uimax").AsScalar().GetFloat();
                }
            }
            return res;
        }

        public static double UiStep(this EffectVariable var)
        {
            double res = -1;
            if (var.GetAnnotationByName("uistep") != null)
            {
                if (var.GetAnnotationByName("uistep").AsScalar() != null)
                {
                    res = var.GetAnnotationByName("uistep").AsScalar().GetFloat();
                }
            }
            return res;
        }

        public static bool IsColor(this EffectVariable var)
        {
            bool res = false;
            if (var.GetAnnotationByName("color") != null)
            {
                if (var.GetAnnotationByName("color").AsScalar() != null)
                {
                    res = var.GetAnnotationByName("color").AsScalar().GetFloat() > 0.5f;
                }
            }
            return res;
        }

        public static bool IsBang(this EffectVariable var)
        {
            bool res = false;
            if (var.GetAnnotationByName("bang") != null)
            {
                if (var.GetAnnotationByName("bang").AsScalar() != null)
                {
                    res = var.GetAnnotationByName("bang").AsScalar().GetFloat() > 0.5f;
                }
            }
            return res;
        }

        public static bool IsTextureMatrix(this EffectVariable var)
        {
            bool res = false;
            if (var.GetAnnotationByName("uvspace") != null)
            {
                if (var.GetAnnotationByName("uvspace").AsScalar() != null)
                {
                    res = var.GetAnnotationByName("uvspace").AsScalar().GetFloat() > 0.5f;
                }
            }
            return res;
        }

        public static bool InvY(this EffectVariable var)
        {
            bool res = false;
            if (var.GetAnnotationByName("invy") != null)
            {
                if (var.GetAnnotationByName("invy").AsScalar() != null)
                {
                    res = var.GetAnnotationByName("invy").AsScalar().GetFloat() > 0.5f;
                }
            }
            return res;
        }

        public static string[] LinkClasses(this EffectVariable var)
        {
            string[] result = new string[0];
            if (var.GetAnnotationByName("linkclass") != null)
            {
                if (var.GetAnnotationByName("linkclass").AsString() != null)
                {
                    result = var.GetAnnotationByName("linkclass").AsString().GetString().Split(",".ToCharArray());
                }
            }
            return result;
        }

        public static PrimitiveTopology Topology(this EffectPass var)
        {
            if (var.GetAnnotationByName("topology") != null)
            {
                if (var.GetAnnotationByName("topology").AsString() != null)
                {
                    try
                    {
                        return (PrimitiveTopology)Enum.Parse(typeof(PrimitiveTopology), var.GetAnnotationByName("topology").AsString().GetString(), true);
                    }
                    catch
                    {
                        return PrimitiveTopology.Undefined;
                    }
                }
            }
            return PrimitiveTopology.Undefined;
        }

        public static string LinkClassesStr(this EffectVariable var)
        {
            string result = String.Empty;
            if (var.GetAnnotationByName("linkclass") != null)
            {
                if (var.GetAnnotationByName("linkclass").AsString() != null)
                {
                    result = var.GetAnnotationByName("linkclass").AsString().GetString();
                }
            }
            return result;
        }

        public static bool Match(this EffectVariable var, IShaderPin shadervar)
        {
            //For pins, if type/elements or now has a semantic, we need to recreate the pin
            bool res = var.GetVariableType().Description.TypeName == shadervar.TypeName
                && var.GetVariableType().Description.Elements == shadervar.Elements
                && var.Description.Name == shadervar.Name
                && var.Description.Semantic =="";

            //Check if need to change shader type
            if (res && shadervar is IMultiTypeShaderPin)
            {
                res = ((IMultiTypeShaderPin)shadervar).ChangeType(var);
            }

            return res;
        }

        public static bool NeedDestroy(this EffectVariable var, IRenderVariable shadervar)
        {
            //For pins, if type/elements or now has a semantic, we need to recreate the pin
            return var.GetVariableType().Description.TypeName != shadervar.TypeName
                || var.GetVariableType().Description.Elements != shadervar.Elements
                || var.Description.Semantic != shadervar.Semantic;
        }

        public static bool NeedDestroy(this EffectVariable var, IWorldRenderVariable shadervar)
        {
            //For pins, if type/elements or now has a semantic, we need to recreate the pin
            return var.GetVariableType().Description.TypeName != shadervar.TypeName
                || var.GetVariableType().Description.Elements != shadervar.Elements
                || var.Description.Semantic != shadervar.Semantic;
        }   
    }
}
