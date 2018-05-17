using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using FeralTic.DX11;
using SlimDX.D3DCompiler;
using SlimDX;

namespace VVVV.DX11.Lib.Effects
{
    public class ImageShaderPassInfo
    {
        private EffectPass effectPass;

        public enum eImageScaleReference { Initial, Previous }

        public bool Mips { get; private set; }
        public bool CustomFormat { get; private set; }
        public Format Format { get; private set; }
        public bool DoScale { get; private set; }
        public eImageScaleReference Reference { get; private set; }
        public bool UseDepth { get; private set; }
        public bool HasState { get; private set; }
        public bool KeepTarget { get; private set; }
        public bool Clear { get; private set; }
        public bool Absolute { get; private set; }

        public int IterationCount { get; private set; }

        public Vector2 ScaleVector { get; private set; }

        public string BlendPreset { get; private set; }
        public string DepthPreset { get; private set; }

        public ImagePassComputeInfo ComputeData { get; private set; }

        public void Apply(DeviceContext context)
        {
            this.effectPass.Apply(context);
        }

        public ImageShaderPassInfo(EffectPass pd)
        {
            this.effectPass = pd;
            this.Mips = false;
            this.CustomFormat = false;
            this.ScaleVector = new Vector2(1, 1);
            this.DoScale = false;
            this.Reference = eImageScaleReference.Previous;
            this.BlendPreset = "";
            this.DepthPreset = "";
            this.UseDepth = false;
            this.HasState = false;
            this.KeepTarget = false;
            this.Absolute = false;
            this.IterationCount = 1;

            this.ComputeData = new ImagePassComputeInfo(pd);

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
                if (var.GetVariableType().Description.Class == ShaderVariableClass.Scalar)
                {
                    float s = var.AsScalar().GetFloat();
                    this.ScaleVector = new Vector2(s, s);
                    this.DoScale = true;
                }
                if (var.GetVariableType().Description.Class == ShaderVariableClass.Vector)
                {
                    Vector4 s = var.AsVector().GetVector();
                    this.ScaleVector = new Vector2(s.X, s.Y);
                    this.DoScale = true;
                }
                var = pd.GetAnnotationByName("absolute");
                if (var.IsValid && var.GetVariableType().Description.Class == ShaderVariableClass.Scalar)
                {
                    this.Absolute = var.AsScalar().GetFloat() > 0.5f;
                }

            }

            var = pd.GetAnnotationByName("initial");
            if (var.IsValid)
            {
                bool b = var.AsScalar().GetFloat() > 0.5f;
                this.Reference = b ? eImageScaleReference.Initial : eImageScaleReference.Previous;
            }

            var = pd.GetAnnotationByName("iterations");
            if (var.IsValid)
            {
                try
                {
                    int i = var.AsScalar().GetInt();

                    this.IterationCount = Math.Max(1, i);
                }
                catch
                {

                }
            }

            this.Clear = pd.GetBoolPassAnnotationByName("clear", this.Clear);
            this.UseDepth = pd.GetBoolPassAnnotationByName("usedepth", this.UseDepth);
            this.KeepTarget = pd.GetBoolPassAnnotationByName("keeptarget", this.KeepTarget);
            this.HasState = pd.GetBoolPassAnnotationByName("hasstate", this.HasState);

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
