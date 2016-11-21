using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.RenderGraph.Model
{
    public class DX11Pin
    {
        public DX11Pin(DX11Node parentnode, IPin hdePin, IPluginIO pluginIO)
        {
            this.ParentNode = parentnode;
            this.HdePin = hdePin;
            this.PluginIO = pluginIO;
        }

        public string Name { get { return this.HdePin.Name; } }
        public DX11Node ParentNode { get; private set; }
        public IPin HdePin { get; private set; }
        public IPluginIO PluginIO { get; private set; }



        //VVVV Info
        //public IDX11InternalResourcePin Instance { get; set; }

    }
}
