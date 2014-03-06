using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using FeralTic.DX11;
using SlimDX.D3DCompiler;

namespace VVVV.DX11.Lib.Effects
{
    public static class EffectPassExtension
    {
        public static int GetIntAnnotation(this EffectPass ext,string name, int defaultvalue)
        {
            EffectVariable var = ext.GetAnnotationByName(name);
            if (var.IsValid)
            {
                return var.AsScalar().GetInt();
            }
            else
            {
                return defaultvalue;
            }
        }

        public static InputElement[] GetStreamOutputLayout(this EffectPass pass, out int vertexsize)
        {
            vertexsize = 0;
            if (pass.GeometryShaderDescription.Variable == null)
            {
                return new InputElement[0];
            }
            else
            {
                EffectShaderVariable gs = pass.GeometryShaderDescription.Variable;
                int outputcount = gs.GetShaderDescription(0).OutputParameterCount;

                InputElement[] elems = new InputElement[outputcount];

                int offset = 0;

                for (int vip = 0; vip < outputcount; vip++)
                {
                    ShaderParameterDescription sd = gs.GetOutputParameterDescription(0, vip);
                    int componentcount = 0;

                    if (sd.UsageMask.HasFlag(RegisterComponentMaskFlags.ComponentX)) { componentcount++; }
                    if (sd.UsageMask.HasFlag(RegisterComponentMaskFlags.ComponentY)) { componentcount++; }
                    if (sd.UsageMask.HasFlag(RegisterComponentMaskFlags.ComponentZ)) { componentcount++; }
                    if (sd.UsageMask.HasFlag(RegisterComponentMaskFlags.ComponentW)) { componentcount++; } 

                    int vsize = 4 * componentcount;

                    string fmt = "";
                    if (componentcount == 1) { fmt = "R32_"; }
                    if (componentcount == 2) { fmt = "R32G32_"; }
                    if (componentcount == 3) { fmt = "R32G32B32_"; }
                    if (componentcount == 4) { fmt = "R32G32B32A32_"; }

                    switch (sd.ComponentType)
                    {
                        case RegisterComponentType.Float32:
                            fmt += "Float";
                            break;
                        case RegisterComponentType.SInt32:
                            fmt += "SInt";
                            break;
                        case RegisterComponentType.UInt32:
                            fmt += "UInt";
                            break; 
                    }

                    Format f = (Format)Enum.Parse(typeof(Format), fmt);

                    InputElement elem = new InputElement(sd.SemanticName, (int)sd.SemanticIndex, f, offset, 0);

                    elems[vip] = elem;

                    offset += vsize;
                    vertexsize += vsize;
                }

                return elems;
            }
        }
    }

    public class ImageComputeData
    {
        public ImageComputeData(EffectPass pass)
        {
            this.Enabled = pass.ComputeShaderDescription.Variable.IsValid;
            this.tX = pass.GetIntAnnotation("tx", 1);
            this.tY = pass.GetIntAnnotation("ty", 1);
            this.tZ = pass.GetIntAnnotation("tz", 1);
        }

        public bool Enabled { get; protected set; }
        public int tX { get; protected set; }
        public int tY { get; protected set; }
        public int tZ { get; protected set; }

        public void Dispatch(DX11RenderContext context, int w, int h)
        {
            context.CurrentDeviceContext.PixelShader.Set(null);

            int tgx = (w + (this.tX - 1)) / this.tX;
            int tgy = (h + (this.tY - 1)) / this.tY;

            context.CurrentDeviceContext.Dispatch(tgx, tgy, 1);
        }
    }


    public class ImageShaderPass
    {
        public enum eImageScaleReference { Initial, Previous }

        public bool Mips { get; protected set; }
        public bool CustomFormat { get; protected set; }
        public Format Format { get; protected set; }
        public bool DoScale { get; protected set; }
        public float Scale { get; protected set; }
        public eImageScaleReference Reference { get; protected set; }
        public bool UseDepth { get; protected set; }
        public bool HasState { get; protected set; }
        public bool KeepTarget { get; protected set; }
        public bool Clear { get; protected set; }

        public string BlendPreset { get; protected set; }
        public string DepthPreset { get; protected set; }

        public ImageComputeData ComputeData { get; protected set; }

        public ImageShaderPass(EffectPass pd)
        {
            this.Mips = false;
            this.CustomFormat = false;
            this.Scale = 1.0f;
            this.DoScale = false;
            this.Reference = eImageScaleReference.Previous;
            this.BlendPreset = "";
            this.DepthPreset = "";
            this.UseDepth = false;
            this.HasState = false;
            this.KeepTarget = false;


            this.ComputeData = new ImageComputeData(pd);

            EffectVariable var = pd.GetAnnotationByName("format");
            if (var.IsValid)
            {
                string fmt = var.AsString().GetString();
                this.CustomFormat = true;
                this.Format = (SlimDX.DXGI.Format)Enum.Parse(typeof(SlimDX.DXGI.Format), fmt, true);
            }

            var = pd.GetAnnotationByName("mips");
            if (var.IsValid)
            {
                bool b = var.AsScalar().GetFloat() > 0.5f;
                this.Mips = b;
            }

            var = pd.GetAnnotationByName("scale");
            if (var.IsValid)
            {
                this.Scale = var.AsScalar().GetFloat();
                this.DoScale = true;
            }

            var = pd.GetAnnotationByName("initial");
            if (var.IsValid)
            {
                bool b = var.AsScalar().GetFloat() > 0.5f;
                this.Reference = b ? eImageScaleReference.Initial : eImageScaleReference.Previous;
            }

            var = pd.GetAnnotationByName("clear");
            if (var.IsValid)
            {
                bool b = var.AsScalar().GetFloat() > 0.5f;
                this.Clear = b;
            }

            var = pd.GetAnnotationByName("usedepth");
            if (var.IsValid)
            {
                bool b = var.AsScalar().GetFloat() > 0.5f;
                this.UseDepth = b;
            }

            var = pd.GetAnnotationByName("keeptarget");
            if (var.IsValid)
            {
                bool b = var.AsScalar().GetFloat() > 0.5f;
                this.KeepTarget = b;
            }

            var = pd.GetAnnotationByName("hasstate");
            if (var.IsValid)
            {
                bool b = var.AsScalar().GetFloat() > 0.5f;
                this.HasState = b;
            }


            var = pd.GetAnnotationByName("blendpreset");
            if (var.IsValid)
            {
                string blend = var.AsString().GetString();
                this.BlendPreset = blend;
            }


            var = pd.GetAnnotationByName("depthpreset");
            if (var.IsValid)
            {
                string depth = var.AsString().GetString();
                this.DepthPreset = depth;
            }
        }
    }
}
