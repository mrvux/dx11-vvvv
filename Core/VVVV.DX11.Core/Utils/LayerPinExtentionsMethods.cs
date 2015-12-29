using FeralTic.DX11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11
{
    public static class LayerPinExtentionsMethods
    {
        public static void RenderParents(this Pin<DX11Resource<DX11Layer>> pin, DX11RenderContext context, DX11RenderSettings settings)
        {
            for (int i = 0; i < pin.SliceCount; i++)
            {
                pin[i][context].Render(context, settings);
            }
        }
    }
}
