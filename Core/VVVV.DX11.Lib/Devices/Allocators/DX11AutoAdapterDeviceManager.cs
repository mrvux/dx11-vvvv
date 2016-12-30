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
    public class DX11AutoAdapterDeviceManager : AbstractDX11RenderContextManager<int>
    {
        private DX11RenderContext context;


        public DX11AutoAdapterDeviceManager(ILogger logger, DX11DisplayManager displaymanager, int adapterid)
            : base(logger, displaymanager)
        {
            SetDevice(logger, displaymanager, adapterid);
        }

        public DX11AutoAdapterDeviceManager(ILogger logger, DX11DisplayManager displaymanager)
            : base(logger, displaymanager)
        {
            bool foundnv;
            int devid = this.DisplayManager.FindNVidia(out foundnv);

            if (!foundnv)
            {
                logger.Log(LogType.Warning, "Did not find NVidia adapter, revert to default");
            }

            SetDevice(logger, displaymanager, devid);
        }

        private void SetDevice(ILogger logger, DX11DisplayManager displaymanager, int adapterid)
        {
            Adapter1 adapter = this.DisplayManager.FindAdapter(adapterid);

            logger.Log(LogType.Message, "Creating device for adapter " + adapter.Description.Description);

#if DEBUG
            try
            {
                this.context = new DX11RenderContext(adapter, this.flags);
            }
            catch
            {
                logger.Log(LogType.Warning, "Could not create Debug device, if you want debug informations make sure DirectX SDK is installed");
                logger.Log(LogType.Warning, "Creating default DirectX 11 device");
                this.flags = DeviceCreationFlags.BgraSupport;
                this.context = new DX11RenderContext(adapter, this.flags);
            }
#else
            this.context = new DX11RenderContext(adapter, this.flags);
#endif

            this.context.Initialize();
            this.contexts.Add(0, this.context);
        }

        protected override int GetDeviceKey(DXGIScreen screen)
        {
            return screen.AdapterId;
        }

        public override DX11RenderContext GetRenderContext(DXGIScreen screen)
        {
            return this.context;
        }

        public override void DestroyContext(DXGIScreen screen)
        {
            //Do nothing here
        }

        public override bool Reallocate
        {
            get { return false; }
        }
    }
}
