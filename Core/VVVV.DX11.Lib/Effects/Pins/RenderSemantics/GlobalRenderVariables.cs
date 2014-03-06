using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Lib.Effects.RenderSemantics;
using SlimDX.Direct3D11;
using VVVV.DX11.Internals;
using SlimDX;

using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;

namespace VVVV.DX11.Lib.Effects.Pins.RenderSemantics
{
    public class ReadBufferRenderVariable : AbstractRenderVariable
    {
        public ReadBufferRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            if (settings.ReadBuffer != null)
            {
                shaderinstance.SetByName(this.Name, settings.ReadBuffer.SRV);
            }
        }
    }

    public class RWBackBufferRenderVariable : AbstractRenderVariable
    {
        public RWBackBufferRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            if (settings.BackBuffer != null)
            {
                EffectVariable counter = this.variable.GetAnnotationByName("counter");
                if (counter != null)
                {
                    float cnt =  counter.AsScalar().GetFloat();
                    shaderinstance.SetByName(this.Name, settings.BackBuffer.UAV,(int)cnt);
                }
                else
                {
                    if (settings.ResetCounter)
                    {
                        shaderinstance.SetByName(this.Name, settings.BackBuffer.UAV, settings.CounterValue);
                    }
                    else
                    {
                        shaderinstance.SetByName(this.Name, settings.BackBuffer.UAV);
                    }
                } 
            }
        }
    }

    public class IntElemSizeRenderVariable : AbstractRenderVariable
    {
        public IntElemSizeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, settings.RenderWidth);
        }
    }

    public class IntViewPortCountRenderVariable : AbstractRenderVariable
    {
        public IntViewPortCountRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, settings.ViewportCount);
        }
    }

    public class IntViewPortIndexRenderVariable : AbstractRenderVariable
    {
        public IntViewPortIndexRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, settings.ViewportIndex);
        }
    }

    public class Float2TargetSizeRenderVariable : AbstractRenderVariable
    {
        public Float2TargetSizeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, new Vector2(settings.RenderWidth, settings.RenderHeight));
        }
    }

    public class Float3TargetSizeRenderVariable : AbstractRenderVariable
    {
        public Float3TargetSizeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, new Vector3(settings.RenderWidth, settings.RenderHeight, settings.RenderDepth));
        }
    }

    public class Float2InvTargetSizeRenderVariable : AbstractRenderVariable
    {
        public Float2InvTargetSizeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            Vector2 v = new Vector2(settings.RenderWidth, settings.RenderHeight);
            v.X = 1.0f / v.X;
            v.Y = 1.0f / v.Y;
            shaderinstance.SetByName(this.Name, v);
        }
    }

    public class Float3InvTargetSizeRenderVariable : AbstractRenderVariable
    {
        public Float3InvTargetSizeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            Vector3 v = new Vector3(settings.RenderWidth, settings.RenderHeight,settings.RenderDepth);
            v.X = 1.0f / v.X;
            v.Y = 1.0f / v.Y;
            v.Z = 1.0f / v.Z;
            shaderinstance.SetByName(this.Name, v);
        }
    }

    public class Float4TargetSizeRenderVariable : AbstractRenderVariable
    {
        public Float4TargetSizeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            Vector2 v = new Vector2(settings.RenderWidth, settings.RenderHeight);
            shaderinstance.SetByName(this.Name, new Vector4(v.X, v.Y, 1.0f / v.X, 1.0f / v.Y));
        }
    }

    public class Float4VolumeSizeRenderVariable : AbstractRenderVariable
    {
        public Float4VolumeSizeRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, new Vector4(settings.RenderWidth, settings.RenderHeight, settings.RenderDepth, 1.0f));
        }
    }

    public class IntDrawCountRenderVariable : AbstractRenderVariable
    {
        public IntDrawCountRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, settings.DrawCallCount);
        }
    }

    public class FloatDrawCountRenderVariable : AbstractRenderVariable
    {
        public FloatDrawCountRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            shaderinstance.SetByName(this.Name, (float)settings.DrawCallCount);
        }
    }

    public class InvFloatDrawCountRenderVariable : AbstractRenderVariable
    {
        public InvFloatDrawCountRenderVariable(EffectVariable var) : base(var) { }

        public override void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
        {
            float val = 1.0f / (float)settings.DrawCallCount;
            shaderinstance.SetByName(this.Name, val);
        }
    }
}
