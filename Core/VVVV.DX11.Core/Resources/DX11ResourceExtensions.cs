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
        public static void CreateIfNull<T>(this ISpread<DX11Resource<T>> spread) where T : IDX11Resource
        {
            for (int i = 0; i < spread.SliceCount; i++)
            {
                if (spread[i] == null)
                {
                    spread[i] = new DX11Resource<T>();
                }
            }
        }

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

        public static void SafeUnlock(this ISpread<DX11Resource<DX11RenderTarget2D>> spread)
        {
            for (int i = 0; i < spread.SliceCount; i++)
            {
                foreach (var key in spread[i].Data.Keys)
                {
                    var value = spread[i][key];
                    key.ResourcePool.Unlock(value);
                }
                spread[i].Data.Clear();
            }
        }

        public static void SafeDispose<T>(this ISpread<DX11Resource<T>> spread, int index) where T : IDX11Resource
        {
            if (spread.SliceCount > 0 && spread[index] != null)
            {
                spread[index].Dispose();
                spread[index] = null;
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
