using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V2;
using System.Reflection;
using SlimDX;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public class FloatArrayShaderPin : AbstractArrayPin<float>
    {
        protected override void UpdateShaderValue(DX11ShaderInstance shaderinstance)
        {
            shaderinstance.SetByName(this.Name, this.array);
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsScalar();
            return (i) => { this.UpdateArray(i); sv.Set(this.array); };
        }

    }

    public class BoolArrayShaderPin : AbstractArrayPin<bool>
    {
        protected override void UpdateShaderValue(DX11ShaderInstance shaderinstance)
        {
            shaderinstance.SetByName(this.Name, this.array);
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsScalar();
            return (i) => { this.UpdateArray(i); sv.Set(this.array); };
        }
    }

    public class IntArrayShaderPin : AbstractArrayPin<int>
    {
        protected override void UpdateShaderValue(DX11ShaderInstance shaderinstance)
        {
            shaderinstance.SetByName(this.Name, this.array);
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsScalar();
            return (i) => { this.UpdateArray(i); sv.Set(this.array); };
        }
    }

    public class Float2ArrayShaderPin : AbstractArrayPin<Vector2>
    {

        protected override void UpdateShaderValue(DX11ShaderInstance shaderinstance)
        {
            DataStream ds = new DataStream(4 * this.array.Length * sizeof(float), true, true);

            for (int i = 0; i < this.array.Length; i++)
            {
                ds.Write<Vector2>(this.array[i]);
                ds.Write(0.0f); ds.Write(0.0f);
            }

            ds.Position = 0;
            shaderinstance.Effect.GetVariableByName(this.Name).AsVector().SetRawValue(ds, (int)ds.Length);
            ds.Dispose();
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsVector();
            return (i) => {  };
        }
    }

    public class Float3ArrayShaderPin : AbstractArrayPin<Vector3>
    {
        protected override void UpdateShaderValue(DX11ShaderInstance shaderinstance)
        {
            DataStream ds = new DataStream(4 * this.array.Length * sizeof(float),true,true);

            for (int i = 0; i < this.array.Length; i++)
            {
                ds.Write<Vector3>(this.array[i]);
                ds.Write(0.0f);
            }
            
            ds.Position = 0;
            shaderinstance.Effect.GetVariableByName(this.Name).SetRawValue(ds, (int)ds.Length);
            ds.Dispose();
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsVector();
            return (i) => { };
        }
    }

    public class Float4ArrayShaderPin : AbstractArrayPin<Vector4>, IMultiTypeShaderPin
    {
        protected override void UpdateShaderValue(DX11ShaderInstance shaderinstance)
        {
            shaderinstance.SetByName(this.Name,this.array);
        }

        public bool ChangeType(EffectVariable var)
        {
            return !var.IsColor();
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsVector();
            return (i) => { };
        }
    }

    public class ColorArrayShaderPin : AbstractArrayPin<Color4>, IMultiTypeShaderPin
    {
        protected override void UpdateShaderValue(DX11ShaderInstance instance)
        {
            instance.SetByName(this.Name, this.array);
        }

        public bool ChangeType(EffectVariable var)
        {
            return var.IsColor();
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsVector();
            return (i) => { };
        }
    }



    public class MatrixArrayShaderPin : AbstractArrayPin<Matrix>
    {
        protected override void UpdateShaderValue(DX11ShaderInstance shaderinstance)
        {
            shaderinstance.Effect.GetVariableByName(this.Name).AsMatrix().SetMatrixArray(this.array, 0, this.array.Length);
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsVector();
            return (i) => { };
        }
    }
}

