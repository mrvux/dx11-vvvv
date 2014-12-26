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
    public delegate void RenderContextCreatedDelegate(DX11RenderContext context);

    public interface IDX11RenderContextManager : IDisposable
    {
        event RenderContextCreatedDelegate RenderContextCreated;
        DX11RenderContext GetRenderContext(DXGIScreen screen);
        void DestroyContext(DXGIScreen screen);
        List<DX11RenderContext> RenderContexts { get; }
        DX11DisplayManager DisplayManager { get; }
        bool Reallocate { get; }
        void EndFrame();
    }


    public abstract class AbstractDX11RenderContextManager<T> : IDX11RenderContextManager
    {
        protected Dictionary<T, DX11RenderContext> contexts = new Dictionary<T, DX11RenderContext>();
        private DX11DisplayManager displaymanager;
        protected DeviceCreationFlags flags;
        private ILogger logger;

        public event RenderContextCreatedDelegate RenderContextCreated;

        public abstract bool Reallocate { get; }

        public AbstractDX11RenderContextManager(ILogger logger, DX11DisplayManager displaymanager)
        {
            this.logger = logger;

            #if DEBUG
            this.flags = DeviceCreationFlags.Debug;
            #else
            this.flags = DeviceCreationFlags.None;
            #endif

            this.flags |= DeviceCreationFlags.BgraSupport;
            this.displaymanager = displaymanager;

        }

        public DX11DisplayManager DisplayManager
        {   
            get { return this.displaymanager; }
        }

        public void EndFrame()
        {
            foreach (DX11RenderContext context in this.RenderContexts)
            {
                context.CleanUp();
                context.CleanUpCS();
            }
        }

        protected abstract T GetDeviceKey(DXGIScreen screen);

        public virtual DX11RenderContext GetRenderContext(DXGIScreen screen)
        {
            this.logger.Log(LogType.Message, "Creating DX11 Render Context");

            T key = this.GetDeviceKey(screen);

            if (!contexts.ContainsKey(key))
            {
                DX11RenderContext ctx;
                #if DEBUG
                try
                {
                    ctx = new DX11RenderContext(this.displaymanager.Factory, screen, this.flags);
                }
                catch
                {
                    this.logger.Log(LogType.Warning, "Could not create debug device, if you want debug informations make sure DirectX SDK is installed");
                    this.logger.Log(LogType.Warning, "Creating default DirectX 11 device");
                    this.flags = DeviceCreationFlags.BgraSupport;
                    ctx = new DX11RenderContext(this.displaymanager.Factory, screen, this.flags);
                }
                #else
                ctx = new DX11RenderContext(this.displaymanager.Factory, screen, this.flags);
                #endif

                ctx.Initialize();

                contexts.Add(key, ctx);
                if (this.RenderContextCreated != null)
                {
                    this.RenderContextCreated(ctx);
                }
            }
            return contexts[key];
        }

        public List<DX11RenderContext> RenderContexts { get { return this.contexts.Values.ToList(); } }

        /// <summary>
        /// Destroys a device from an adapter
        /// </summary>
        /// <param name="adapter">Adapter index</param>
        public virtual void DestroyContext(DXGIScreen screen)
        {
            T key = this.GetDeviceKey(screen);

            if (contexts.ContainsKey(key))
            {
                contexts[key].Dispose();
                contexts.Remove(key);
            }
        }

        public void Dispose()
        {
            foreach (DX11RenderContext ctx in this.RenderContexts)
            {
                try
                {
                    ctx.CurrentDeviceContext.ClearState();
                    ctx.CurrentDeviceContext.Flush();
                }
                catch { }

                try
                {
                    ctx.Dispose();
                }
                catch { }
            }
        }
    }

    public class DX11PerAdapterDeviceManager : AbstractDX11RenderContextManager<int>
    {

        public DX11PerAdapterDeviceManager(ILogger logger, DX11DisplayManager displaymanager) : base(logger, displaymanager)
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


    public class DX11PerMonitorDeviceManager : AbstractDX11RenderContextManager<string>
    {
        public DX11PerMonitorDeviceManager(ILogger logger, DX11DisplayManager displaymanager) : base(logger,displaymanager) { }

        protected override string GetDeviceKey(DXGIScreen screen)
        {
            return screen.Monitor.Description.Name;
        }

        public override bool Reallocate
        {
            get { return true; }
        }
    }

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
