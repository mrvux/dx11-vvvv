using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins;
using VVVV.Hosting.Pins.Input;
using VVVV.DX11;
using FeralTic.Resources;
using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public abstract class TextureArrayShaderPin<U, R> : AbstractArrayPin<DX11Resource<U>>
        where R : Resource
        where U : IDX11Resource
    {
        protected abstract ShaderResourceView GetSRV(DX11ShaderInstance shaderinstance,int bin,int slice);

        public override void SetVariable(DX11ShaderInstance shaderinstance, int slice)
        {
            
            //ISpread<U> f = this.pin[slice];
            ShaderResourceView[] data = new ShaderResourceView[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                try
                {
                    data[i] = this.GetSRV(shaderinstance, slice, i);
                }
                catch
                {
                    data[i] = null;
                }
            }
            shaderinstance.SetByName(this.Name, data);
        }


        protected override void UpdateShaderValue(DX11ShaderInstance shaderinstance)
        {
            
        }
    }

    public class Texture2DArrayShaderPin : TextureArrayShaderPin<DX11Texture2D, Texture2D>
    {

        protected override ShaderResourceView GetSRV(DX11ShaderInstance shaderinstance, int bin, int slice)
        {
            return  this.pin[bin][slice][shaderinstance.RenderContext].SRV;
        }
    }

    /*public class Texture1DArrayShaderPin : TextureArrayShaderPin<DX11Texture1D, Texture1D>
    {
        public Texture1DArrayShaderPin(EffectVariable var, IPluginHost host, IIOFactory factory) : base(var, host, factory) { }

        protected override ShaderResourceView GetSRV(int binslice, int dataslice)
        {
            try
            {
                return this.pin[binslice][dataslice][this.RenderContext.Device].SRV;
            }
            catch
            {
                return null;
            }
        }
    }*/

    
    /*public class Texture3DArrayShaderPin : TextureArrayShaderPin<DX11Texture3D, Texture3D>
    {
        public Texture3DArrayShaderPin(EffectVariable var, IPluginHost host, IIOFactory factory) : base(var, host, factory) { }

        protected override ShaderResourceView GetSRV(int binslice, int dataslice)
        {
            try
            {
                return this.pin[binslice][dataslice][this.RenderContext.Device].SRV;
            }
            catch
            {
                return null;
            }
        }
    }*/
}
