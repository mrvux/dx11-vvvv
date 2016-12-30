using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Lib.Utils
{
    public static class SpreadUtils
    {
        public static T GetItem<T>(ISpread<T> spread, int idx)
        {
            return spread[idx];
        }
    }
}
