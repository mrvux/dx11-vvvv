using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.RenderGraph.Model
{
    public class DX11VirtualConnection
    {
        public IPin sourcePin;
        public IPin sinkPin;
        public DX11Node sourceNode;

        public DX11VirtualConnection(IPin sinkPin, IPin sourcePin, DX11Node sourceNode)
        {
            this.sourcePin = sourcePin;
            this.sourceNode = sourceNode;
            this.sinkPin = sinkPin;
        }


    }
}
