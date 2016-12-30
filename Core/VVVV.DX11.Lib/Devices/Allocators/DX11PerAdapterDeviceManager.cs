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
    public class DX11PerAdapterDeviceManager : AbstractDX11RenderContextManager<int>
    {
        public DX11PerAdapterDeviceManager(ILogger logger, DX11DisplayManager displaymanager)
            : base(logger, displaymanager)
        {
        }

        protected override int GetDeviceKey(DXGIScreen screen)
        {
            return screen.AdapterId;
        }

        public override bool Reallocate
        {
            get { return true; }
        }
    }
}
