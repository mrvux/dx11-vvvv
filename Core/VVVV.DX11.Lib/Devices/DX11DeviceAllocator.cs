using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using FeralTic.DX11;
using FeralTic.Utils;

namespace VVVV.DX11.Lib.Devices
{
    public delegate void RenderContextDisposingDelegate(DX11RenderContext device);

    public class DX11DeviceAllocator
    {
        private IDX11RenderContextManager devicemanager;

        private List<IAttachableWindow> renderwindows;

        public event RenderContextDisposingDelegate RenderContextDisposing;

        public DX11DeviceAllocator(IDX11RenderContextManager devicemgr)
        {
            this.devicemanager = devicemgr;
            this.renderwindows = new List<IAttachableWindow>();
        }

        public void AddRenderWindow(IAttachableWindow window)
        {
            if (!this.renderwindows.Contains(window))
            {
                this.renderwindows.Add(window);
            }
        }

        public void RemoveRenderWindow(IAttachableWindow window)
        {
            if (this.renderwindows.Contains(window))
            {
                this.renderwindows.Remove(window);
            }
        }

        public List<DX11RenderContext> RenderContexts
        {
            get { return this.devicemanager.RenderContexts; }
        }

        public void Reallocate()
        {
            if (this.devicemanager.Reallocate)
            {

                //Refresh Display List
                //this.devicemanager.DisplayManager.Refresh();

                //TODO : Also kills device if monitor not here anymore

                //Get a list of all devices for renderwindows
                foreach (IDX11RenderWindow window in this.renderwindows)
                {
                    DXGIScreen screen = this.GetScreen(window);

                    //Assign primary window device 
                    window.AttachContext(this.devicemanager.GetRenderContext(screen));
                }

                //Get List of existing devices
                List<DX11RenderContext> unused = new List<DX11RenderContext>();

                foreach (DX11RenderContext device in this.devicemanager.RenderContexts)
                {
                    bool found = false;
                    foreach (IDX11RenderWindow window in this.renderwindows)
                    {
                        if (device == window.RenderContext) { found = true; } //Device in use
                    }

                    if (!found) { unused.Add(device); }
                }

                //Kill unused devices
                foreach (DX11RenderContext device in unused)
                {
                    //Send event device gonna be disposed
                    this.OnDeviceDisposing(device);

                    this.devicemanager.DestroyContext(device.Screen);
                }
            }
            else
            {
                foreach (IDX11RenderWindow window in this.renderwindows)
                {
                    window.AttachContext(this.devicemanager.RenderContexts[0]);
                }
            }
        }

        private DXGIScreen GetScreen(IDX11RenderWindow window)
        {
            Screen screen = Screen.FromHandle(window.WindowHandle);
            return this.devicemanager.DisplayManager.FindAdapter(screen);
        }

        #region Device Disposing
        protected void OnDeviceDisposing(DX11RenderContext ctx)
        {
            if (this.RenderContextDisposing != null)
            {
                this.RenderContextDisposing(ctx);
            }
        }
        #endregion

        #region Get All DX11 Render Windows
        public List<IDX11RenderWindow> GetWindows(DX11RenderContext ctx)
        {
            List<IDX11RenderWindow> result = new List<IDX11RenderWindow>();
            foreach (IDX11RenderWindow window in this.renderwindows)
            {
                if (window.RenderContext == ctx)
                {
                    result.Add(window);
                }
            }

            return result;
        }
        #endregion
    }
}
