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
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Lib.Effects.Pins.RenderSemantics
{
    public class ReadBufferRenderVariable : AbstractRenderVariable
    {
        public ReadBufferRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsResource();
            return (s) => sv.SetResource(s.ReadBuffer.SRV);
        }
    }

    public class ReadOnlyDepthRenderVariable : AbstractRenderVariable
    {
        public ReadOnlyDepthRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsResource();
            return (s) =>
            {
                if (shader.RenderContext.RenderTargetStack.Current.ReadonlyDepth)
                {
                    var ds = shader.RenderContext.RenderTargetStack.Current.DepthStencil;
                    sv.SetResource(((IDX11ReadableResource)ds).SRV);
                }
                else
                {
                    sv.SetResource(null);
                }
            };
        }
    }


    public class RWBackBufferRenderVariable : AbstractRenderVariable
    {
        public RWBackBufferRenderVariable(EffectVariable var) : base(var) { }

        private void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings)
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

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            return (s) => this.Apply(shader, s);
        }
    }

    public class IntElemSizeRenderVariable : AbstractRenderVariable
    {
        public IntElemSizeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (s) => sv.Set(s.RenderWidth);
        }
    }

    public class IntViewPortCountRenderVariable : AbstractRenderVariable
    {
        public IntViewPortCountRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (s) => sv.Set(s.ViewportCount);
        }
    }

    public class IntViewPortIndexRenderVariable : AbstractRenderVariable
    {
        public IntViewPortIndexRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (s) => sv.Set(s.ViewportIndex);
        }
    }

    public class FloatLayerOpacityRenderVariable : AbstractRenderVariable
    {
        public FloatLayerOpacityRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (s) => sv.Set(s.LayerOpacity);
        }
    }

    public class Float2TargetSizeRenderVariable : AbstractRenderVariable
    {
        public Float2TargetSizeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsVector();
            return (s) => sv.Set(new Vector2(s.RenderWidth, s.RenderHeight));
        }
    }

    public class Float3TargetSizeRenderVariable : AbstractRenderVariable
    {
        public Float3TargetSizeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsVector();
            return (s) => sv.Set(new Vector3(s.RenderWidth, s.RenderHeight, s.RenderDepth));
        }
    }

    public class Float2InvTargetSizeRenderVariable : AbstractRenderVariable
    {
        public Float2InvTargetSizeRenderVariable(EffectVariable var) : base(var) { }

        private Vector2 GetVector(DX11RenderSettings settings)
        {
            Vector2 v = new Vector2(settings.RenderWidth, settings.RenderHeight);
            v.X = 1.0f / v.X;
            v.Y = 1.0f / v.Y;
            return v;
        }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsVector();
            return (s) => sv.Set(this.GetVector(s));
        }
    }

    public class Float3InvTargetSizeRenderVariable : AbstractRenderVariable
    {
        public Float3InvTargetSizeRenderVariable(EffectVariable var) : base(var) { }

        private Vector3 GetVector(DX11RenderSettings settings)
        {
            Vector3 v = new Vector3(settings.RenderWidth, settings.RenderHeight, settings.RenderDepth);
            v.X = 1.0f / v.X;
            v.Y = 1.0f / v.Y;
            v.Z = 1.0f / v.Z;
            return v;
        }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsVector();
            return (s) => sv.Set(this.GetVector(s));
        }
    }

    public class Float4TargetSizeRenderVariable : AbstractRenderVariable
    {
        public Float4TargetSizeRenderVariable(EffectVariable var) : base(var) { }

        private Vector4 GetVector(DX11RenderSettings settings)
        {
            Vector2 v = new Vector2(settings.RenderWidth, settings.RenderHeight);
            return new Vector4(v.X, v.Y, 1.0f / v.X, 1.0f / v.Y);
        }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsVector();
            return (s) => sv.Set(this.GetVector(s));
        }
    }

    public class Float4VolumeSizeRenderVariable : AbstractRenderVariable
    {
        public Float4VolumeSizeRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsVector();
            return (s) => sv.Set(new Vector4(s.RenderWidth, s.RenderHeight, s.RenderDepth, 1.0f));
        }
    }

    public class IntDrawCountRenderVariable : AbstractRenderVariable
    {
        public IntDrawCountRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (s) => sv.Set(s.DrawCallCount);
        }
    }

    public class FloatDrawCountRenderVariable : AbstractRenderVariable
    {
        public FloatDrawCountRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (s) => sv.Set((float)s.DrawCallCount);
        }
    }

    public class InvFloatDrawCountRenderVariable : AbstractRenderVariable
    {
        public InvFloatDrawCountRenderVariable(EffectVariable var) : base(var) { }

        public override Action<DX11RenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var sv = shader.Effect.GetVariableByName(this.Name).AsScalar();
            return (s) => sv.Set(1.0f / (float)s.DrawCallCount);
        }
    }
}
