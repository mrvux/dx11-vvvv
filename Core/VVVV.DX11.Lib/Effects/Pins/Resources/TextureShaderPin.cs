using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D11;

using VVVV.Hosting.Pins;
using VVVV.DX11.Lib;
using VVVV.DX11.Lib.Effects.Pins;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Internals.Effects.Pins
{

    public class Texture1DShaderPin : ResourceShaderPin<DX11Texture1D, Texture1D>
    {
        protected override ShaderResourceView GetSRV(DX11RenderContext context,int slice)
        {
            try
            {
                return this.pin[slice][context].SRV;
            }
            catch
            {
                return null;
            }
        }
    }

    public class Texture2DShaderPin : ResourceShaderPin<DX11Texture2D, Texture2D>
    {

        protected override ShaderResourceView GetSRV(DX11RenderContext context,int slice)
        {
            try
            {
                if (this.pin[slice].Contains(context))
                {
                    return this.pin[slice][context].SRV;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        protected override ShaderResourceView GetDefaultSRV(DX11RenderContext context)
        {
            return context.DefaultTextures.WhiteTexture.SRV;
        }
    }

    public class TextureCubeShaderPin : ResourceShaderPin<DX11Texture2D, Texture2D>
    {
        protected override ShaderResourceView GetSRV(DX11RenderContext context, int slice)
        {
            try
            {
                return this.pin[slice][context].SRV;
            }
            catch
            {
                return null;
            }
        }

        protected override ShaderResourceView GetDefaultSRV(DX11RenderContext context)
        {
            return context.DefaultTextures.WhiteTexture.SRV;
        }
    }

    public class Texture3DShaderPin : ResourceShaderPin<DX11Texture3D, Texture3D>
    {
        protected override ShaderResourceView GetSRV(DX11RenderContext context, int slice)
        {
            try
            {
                return this.pin[slice][context].SRV;
            }
            catch
            {
                return null;
            }

        }
    }
}
