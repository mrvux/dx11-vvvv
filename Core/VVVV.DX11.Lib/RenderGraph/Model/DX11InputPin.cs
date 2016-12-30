using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.DX11.RenderGraph.Model
{
    public class DX11InputPin : DX11Pin
    {
        public DX11InputPin(DX11Node parentnode)
            : base(parentnode) 
        {
            this.ParentNode.InputPins.Add(this);
        }

        public bool IsConnected { get { return this.ParentPin != null; } }

        public DX11OutputPin ParentPin { get; set; }

        public void Disconnect(DX11OutputPin op)
        {
            if (op == this.ParentPin)
            {
                op.ChildrenPins.Remove(this);
                this.ParentPin = null;
            }
        }

        public void Connect(DX11OutputPin op)
        {
            this.ParentPin = op;
            op.ChildrenPins.Add(this);
        }
    }
}
