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

        public override void SetVariable(DX11ShaderInstance shaderinstance, int slice)
        {
            if (!uvspace)
            {
                shaderinstance.SetByName(this.Name, this.pin[slice]);
            }
            else
            {
                Matrix m = pin[slice];
                //m.M42 = -m.M42;
                m = Matrix.Translation(-0.5f, -0.5f, 0.0f) * m * Matrix.Translation(0.5f, 0.5f, 0.0f);
                
                shaderinstance.SetByName(this.Name, m);
            }
        }

        protected override void SetDefault(InputAttribute attr, EffectVariable var) 
        {
            this.uvspace = var.IsTextureMatrix();
        }

        protected override bool RecreatePin(EffectVariable var)
        {
            this.uvspace = var.IsTextureMatrix();
            //Just pick up space, and return same value (no need to kill pin)
            return base.RecreatePin(var);
        }
    }
}

