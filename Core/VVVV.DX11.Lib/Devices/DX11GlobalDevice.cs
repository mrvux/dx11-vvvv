using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Lib.RenderGraph;

namespace VVVV.DX11.Lib.Devices
{
    public static class DX11GlobalDevice
    {
        public static IDX11RenderContextManager DeviceManager { get; set; }
        public static DX11RenderManager RenderManager { get; set; }

        public static int PendingPinsCount { get; set; }
        public static int PendingLinksCount { get; set; }

        public static void Begin()
        {
            if (OnBeginRender != null)
            {
                OnBeginRender(null, new EventArgs());
            }
        }

        public static void End()
        {
            if (OnEndRender != null)
            {
                OnEndRender(null, new EventArgs());
            }
        }

        public static event EventHandler OnBeginRender;
        public static event EventHandler OnEndRender;
    }
}
