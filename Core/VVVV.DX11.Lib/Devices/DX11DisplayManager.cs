using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.DXGI;
using FeralTic.Utils;


namespace VVVV.DX11.Lib.Devices
{
    /// <summary>
    /// Manages displays/adapters from windowed controls
    /// </summary>
    public class DX11DisplayManager
    {
        
        private List<DXGIScreen> screens = new List<DXGIScreen>();

        public Factory1 Factory { get; private set; }

        public List<DXGIScreen> Screens { get { return this.screens; } }

        public int AdapterCount { get; protected set; }

        public DX11DisplayManager()
        {
            this.Factory = new Factory1();

            this.AdapterCount = this.Factory.GetAdapterCount1();

            //Scan once on creation
            this.Refresh();
        }

        public Adapter1 FindAdapter(int index)
        {
            if (index >= 0 && index < this.Factory.GetAdapterCount1())
            {
                return this.Factory.GetAdapter1(index);
            }

            return null;
        }

        public int FindNVidia(out bool found)
        {
            for (int i= 0; i < this.Factory.GetAdapterCount1(); i++)
            {
                var adp = this.Factory.GetAdapter1(i);

                if (adp.Description1.Description.ToLower().Contains("nvidia"))
                {
                    found = true;
                    return i;
                }
            }

            found = false;
            return 0;
        }

        public void Refresh()
        {
            foreach (DXGIScreen scr in this.screens)
            {
                scr.Adapter.Dispose();
                scr.Monitor.Dispose();
            }
            screens.Clear();

            for (int i = 0; i < this.Factory.GetAdapterCount1(); i++)
            {
                Adapter1 adapter = this.Factory.GetAdapter1(i);

                for (int j = 0; j < adapter.GetOutputCount(); j++)
                {
                    Output output = adapter.GetOutput(j);

                    DXGIScreen screen = new DXGIScreen();
                    screen.Adapter = adapter;
                    screen.AdapterId = i;
                    screen.Monitor = output;
                    screen.MonitorId = j;

                    screens.Add(screen);
                }
            }
        }

        public DXGIScreen FindAdapter(Screen wscreen, bool refresh = false)
        {
            if (refresh) { this.Refresh(); }

            foreach (DXGIScreen screen in this.screens)
            {
                if (screen.Monitor.Description.Name.EndsWith(wscreen.DeviceName))
                {
                    return screen;
                }
            }

            //If not found force a refresh and try again
            if (!refresh) { this.Refresh(); }

            foreach (DXGIScreen screen in this.screens)
            {
                if (screen.Monitor.Description.Name.EndsWith(wscreen.DeviceName))
                {
                    return screen;
                }
            }

            //Blank object if not found
            return new DXGIScreen();

        }

    }
}
