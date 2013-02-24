using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using FeralTic.DX11;

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

        public ImageComputeData ComputeData { get; protected set; }

        public ImageShaderPass(EffectPass pd)
        {
            this.Mips = false;
            this.CustomFormat = false;
            this.Scale = 1.0f;
            this.DoScale = false;
            this.Reference = eImageScaleReference.Previous;

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
        }


    }
}
