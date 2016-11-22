using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Lib.Effects.RenderSemantics;
using VVVV.DX11.Internals;

using SlimDX.Direct3D11;
using SlimDX;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;


namespace VVVV.DX11.Lib.Effects.Pins.RenderSemantics
{
    public class ObjectBMinRenderVariable : AbstractWorldRenderVariable
    {
        private Vector3 vec = new Vector3(-0.5f, -0.5f, -0.5f);
        public ObjectBMinRenderVariable(EffectVariable var) : base(var) { }

        private Vector3 GetBoundingBox(DX11ObjectRenderSettings obj)
        {
            if (obj.Geometry != null)
            {
                if (obj.Geometry.HasBoundingBox)
                {
                    return obj.Geometry.BoundingBox.Minimum;
                }
                else
                {
                    return vec;
                }
            }
            else
            {
                return vec;
            }
        }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsVector();
            return (settings, obj) => effectVar.Set(this.GetBoundingBox(obj));
        }
    }

    public class ObjectBMaxRenderVariable : AbstractWorldRenderVariable
    {
        private Vector3 vec = new Vector3(0.5f,-0.5f, 0.5f);
        public ObjectBMaxRenderVariable(EffectVariable var) : base(var) { }

        private Vector3 GetBoundingBox(DX11ObjectRenderSettings obj)
        {
            if (obj.Geometry != null)
            {
                if (obj.Geometry.HasBoundingBox)
                {
                    return obj.Geometry.BoundingBox.Maximum;
                }
                else
                {
                    return vec;
                }
            }
            else
            {
                return vec;
            }
        }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsVector();
            return (settings, obj) => effectVar.Set(this.GetBoundingBox(obj));
        }
    }

    public class ObjectBScaleRenderVariable : AbstractWorldRenderVariable
    {
        private Vector3 vec = new Vector3(1, 1, 1);
        public ObjectBScaleRenderVariable(EffectVariable var) : base(var) { }

        private Vector3 GetBoundingBox(DX11ObjectRenderSettings obj)
        {
            if (obj.Geometry != null)
            {
                if (obj.Geometry.HasBoundingBox)
                {
                    return obj.Geometry.BoundingBox.Maximum - obj.Geometry.BoundingBox.Minimum;
                }
                else
                {
                    return vec;
                }
            }
            else
            {
                return vec;
            }
        }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsVector();
            return (settings, obj) => effectVar.Set(this.GetBoundingBox(obj));
        }
    }

    public class ObjectUnitTransformRenderVariable : AbstractWorldRenderVariable
    {
        private Matrix m = Matrix.Identity;
        public ObjectUnitTransformRenderVariable(EffectVariable var) : base(var) { }

        private Matrix GetBoundingBox(DX11ObjectRenderSettings obj)
        {
            if (obj.Geometry != null)
            {
                if (obj.Geometry.HasBoundingBox)
                {
                    Vector3 scale = obj.Geometry.BoundingBox.Maximum - obj.Geometry.BoundingBox.Minimum;
                    scale.X = scale.X != 0.0f ? 1.0f / scale.X : 1.0f;
                    scale.Y = scale.Y != 0.0f ? 1.0f / scale.Y : 1.0f;
                    scale.Z = scale.Z != 0.0f ? 1.0f / scale.Z : 1.0f;

                    return Matrix.Scaling(scale);
                }
                else
                {
                    return m;
                }
            }
            else
            {
                return m;
            }
        }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (settings, obj) => effectVar.SetMatrix(this.GetBoundingBox(obj));
        }
    }

    public class ObjectSdfTransformRenderVariable : AbstractWorldRenderVariable
    {
        private Matrix m = Matrix.Identity;
        public ObjectSdfTransformRenderVariable(EffectVariable var) : base(var) { }

        private Matrix GetBoundingBox(DX11ObjectRenderSettings obj)
        {
            if (obj.Geometry != null)
            {
                if (obj.Geometry.HasBoundingBox)
                {
                    Vector3 min = obj.Geometry.BoundingBox.Minimum;

                    Vector3 scale = obj.Geometry.BoundingBox.Maximum - obj.Geometry.BoundingBox.Minimum;
                    scale.X = scale.X != 0.0f ? scale.X : 1.0f;
                    scale.Y = scale.Y != 0.0f ? scale.Y : 1.0f;
                    scale.Z = scale.Z != 0.0f ? scale.Z : 1.0f;

                    Matrix m = Matrix.Scaling(scale);

                    m.M41 = min.X;
                    m.M42 = min.Y;
                    m.M43 = min.Z;
                    return Matrix.Invert(m);
                }
                else
                {
                    return m;
                }
            }
            else
            {
                return m;
            }
        }

        public override Action<DX11RenderSettings, DX11ObjectRenderSettings> CreateAction(DX11ShaderInstance shader)
        {
            var effectVar = shader.Effect.GetVariableByName(this.Name).AsMatrix();
            return (settings, obj) => effectVar.SetMatrix(this.GetBoundingBox(obj));
        }
    }
}
