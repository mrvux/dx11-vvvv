using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11
{
    public static class DX11ResourceExtensions
    {
        public static void DisposeSpread<T>(this ISpread<T> spread) where T : IDisposable
        {
            for (int i = 0; i < spread.SliceCount; i++)
            {
                if (spread[i] != null)
                {
                    spread[i].Dispose();
                    spread[i] = default(T);
                }
            }
        }

        public static void SafeDisposeAll<T>(this ISpread<DX11Resource<T>> spread) where T : IDX11Resource
        {
            for (int i = 0; i < spread.SliceCount; i++)
            {
                if (spread[i] != null)
                {
                    spread[i].Dispose();
                    spread[i] = null;
                }
            }
        }

        public static void SafeDisposeAll<T>(this ISpread<DX11Resource<T>> spread, DX11RenderContext context) where T : IDX11Resource
        {
            for (int i = 0; i < spread.SliceCount; i++)
            {
                if (spread[i] != null)
                {
                    spread[i].Dispose(context);
                }
            }
        }
    }
}
