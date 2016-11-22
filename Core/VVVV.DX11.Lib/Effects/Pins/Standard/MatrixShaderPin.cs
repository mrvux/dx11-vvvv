using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.Pins.Input;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using SlimDX;
using VVVV.DX11.Lib.Effects.Pins;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public class MatrixShaderPin : AbstractValuePin<Matrix>
    {
        private bool uvspace;
        private bool invy;

        private Matrix GetMatrix(int slice)
        {
            if (!uvspace)
            {
                return this.pin[slice];
            }
            else
            {
                Matrix m = pin[slice];
                if (!this.invy)
                {
                    m = Matrix.Translation(-0.5f, -0.5f, 0.0f) * Matrix.Scaling(1, -1, 1) * m;
                    m *= Matrix.Translation(0.5f, 0.5f, 0.0f) * Matrix.Scaling(1, -1, 1) * Matrix.Translation(0, 1, 0);
                }
                else
                {
                    m = Matrix.Translation(-0.5f, -0.5f, 0.0f) * m;
                    m *= Matrix.Translation(0.5f, 0.5f, 0.0f);
                }
                return m;
            }
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsMatrix();
            return (i) => { sv.SetMatrix(this.GetMatrix(i)); };
        }

        protected override void SetDefault(InputAttribute attr, EffectVariable var) 
        {
            this.uvspace = var.IsTextureMatrix();
        }

        protected override bool RecreatePin(EffectVariable var)
        {
            this.uvspace = var.IsTextureMatrix();
            this.invy = var.InvY();
            //Just pick up space, and return same value (no need to kill pin)
            return base.RecreatePin(var);
        }
    }
}

