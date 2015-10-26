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
    public class DX11AllAdapterDeviceManager : AbstractDX11RenderContextManager<int>
    {

        public DX11AllAdapterDeviceManager(ILogger logger, DX11DisplayManager displaymanager)
            : base(logger, displaymanager)
        {
            for (int i = 0; i < displaymanager.Factory.GetAdapterCount1(); i++)
            {
                SetDevice(logger, displaymanager, i);
            }
        }


        private void SetDevice(ILogger logger, DX11DisplayManager displaymanager, int adapterid)
        {
            Adapter1 adapter = this.DisplayManager.FindAdapter(adapterid);

            logger.Log(LogType.Message, "Creating device for adapter " + adapter.Description.Description);

            DX11RenderContext context;

#if DEBUG
            try
            {
                context = new DX11RenderContext(adapter, this.flags);
            }
            catch
            {
                logger.Log(LogType.Warning, "Could not create Debug device, if you want debug informations make sure DirectX SDK is installed");
                logger.Log(LogType.Warning, "Creating default DirectX 11 device");
                this.flags = DeviceCreationFlags.BgraSupport;
                context = new DX11RenderContext(adapter, this.flags);
            }
#else
            context = new DX11RenderContext(adapter, this.flags);
#endif

            context.Initialize();
            this.contexts.Add(adapterid, context);
        }

        protected override int GetDeviceKey(DXGIScreen screen)
        {
            return screen.AdapterId;
        }

        public override DX11RenderContext GetRenderContext(DXGIScreen screen)
        {
            return this.contexts[0];
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
