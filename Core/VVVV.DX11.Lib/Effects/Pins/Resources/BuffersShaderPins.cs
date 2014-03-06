using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;
using SlimDX.Direct3D11;
using VVVV.DX11.Lib;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Resources;
using FeralTic.DX11;


namespace VVVV.DX11.Lib.Effects.Pins.Resources
{
    public class ReadableStructuredBufferShaderPin : ResourceShaderPin<IDX11ReadableStructureBuffer, SlimDX.Direct3D11.Buffer>
    {
        protected override ShaderResourceView GetSRV(DX11RenderContext context, int slice)
        {
            if (this.pin[slice] == null)
            {
                return null;
            }
            else
            {
                if (!this.pin[slice].Contains(context))
                {
                    return null;
                }
                else
                {
                    IDX11ReadableStructureBuffer sb = this.pin[slice][context];
                    return sb != null ? sb.SRV : null;
                }
            }
        }
    }

    public class ReadableBufferShaderPin : ResourceShaderPin<IDX11ReadableResource, SlimDX.Direct3D11.Buffer>
    {
        protected override ShaderResourceView GetSRV(DX11RenderContext context, int slice)
        {
            if (this.pin[slice] == null)
            {
                return null;
            }
            else
            {
                if (!this.pin[slice].Contains(context))
                {
                    return null;
                }
                else
                {
                    IDX11ReadableResource sb = this.pin[slice][context];
                    return sb != null ? sb.SRV : null;
                }
            }
        }
    }
}
