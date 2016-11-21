using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.RenderGraph.Model
{
    public class DX11OutputPin : DX11Pin
    {
        private bool isFeedBackPin;

        public DX11OutputPin(DX11Node parentnode, IPin hdePin, IPluginIO pluginIO) : base(parentnode, hdePin, pluginIO) 
        {
            this.ChildrenPins = new List<DX11InputPin>();
            this.ParentNode.OutputPins.Add(this);

            INodeOut nodeout = (INodeOut)this.PluginIO;
            this.isFeedBackPin = nodeout.AllowFeedback;
        }

        public List<DX11InputPin> ChildrenPins { get; private set; }

        public bool IsFeedBackPin
        {
            get { return this.isFeedBackPin; }
        }

    }
}
