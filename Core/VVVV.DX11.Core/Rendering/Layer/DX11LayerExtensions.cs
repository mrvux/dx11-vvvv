using FeralTic.DX11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                buffer[i][context].Render(context, settings);
            }
        }
    }
}
