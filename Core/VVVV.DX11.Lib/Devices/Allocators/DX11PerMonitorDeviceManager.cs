using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;

using FeralTic.DX11;
using FeralTic.Utils;
using VVVV.Core.Logging;
using SlimDX.DXGI;


namespace VVVV.DX11.Lib.Devices
{
    public class DX11PerMonitorDeviceManager : AbstractDX11RenderContextManager<string>
    {
        public DX11PerMonitorDeviceManager(ILogger logger, DX11DisplayManager displaymanager) : base(logger, displaymanager) { }

        protected override string GetDeviceKey(DXGIScreen screen)
        {
            return screen.Monitor.Description.Name;
        }

        public override bool Reallocate
        {
            get { return true; }
        }
    }
}
