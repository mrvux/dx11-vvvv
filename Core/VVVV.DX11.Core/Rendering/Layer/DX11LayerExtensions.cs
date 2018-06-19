using FeralTic.DX11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11
{
    public static class DX11LayerExtensions
    {
        public static void RenderAll(this ISpread<DX11Resource<DX11Layer>> layer, DX11RenderContext context, DX11RenderSettings settings)
        {
            var buffer = layer.Stream.Buffer;
            for (int i = 0; i < layer.SliceCount; i++)
            {
                buffer[i][context]?.Render(context, settings);
            }
        }

        public static void RenderSlice(this ISpread<DX11Resource<DX11Layer>> layer, DX11RenderContext context, DX11RenderSettings settings, int slice)
        {
            layer[slice][context]?.Render(context, settings);
        }

        public static void RenderAllWithLog(this ISpread<DX11Resource<DX11Layer>> layer, DX11RenderContext context, DX11RenderSettings settings, ILogger logger)
        {
            var buffer = layer.Stream.Buffer;
            for (int i = 0; i < layer.SliceCount; i++)
            {
                try
                {
                    buffer[i][context].Render(context, settings);
                }
                catch (Exception ex)
                {
                    logger.Log(ex);
                }
                
            }
        }
    }
}
