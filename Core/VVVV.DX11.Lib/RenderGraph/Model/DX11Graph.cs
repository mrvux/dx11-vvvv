using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2.Graph;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

namespace VVVV.DX11.RenderGraph.Model
{
    public class DX11Graph
    {
        public DX11Graph()
        {
            this.Nodes = new List<DX11Node>();

        }

        public List<DX11Node> Nodes { get; set; }

        public DX11Node FindNode(INode2 hdenode)
        {
            foreach (DX11Node n in this.Nodes)
            {
                if (n.HdeNode == hdenode.InternalCOMInterf)
                {
                    return n;
                }
            }
            return null;
        }

        public DX11Node FindNode(INode hdenode)
        {
            foreach (DX11Node n in this.Nodes)
            {
                if (n.HdeNode == hdenode)
                {
                    return n;
                }
            }
            return null;
        }

        public DX11Node FindNode(IPluginHost host)
        {
            foreach (DX11Node n in this.Nodes)
            {
                if (n.Hoster == host)
                {
                    return n;
                }
            }
            return null;
        }

        public DX11Pin FindPin(IPin hdepin)
        {
            foreach (DX11Node n in this.Nodes)
            {
                foreach (DX11InputPin i in n.InputPins)
                {
                    if (i.HdePin == hdepin)
                    {
                        return i;
                    }
                }
                foreach (DX11OutputPin i in n.OutputPins)
                {
                    if (i.HdePin == hdepin)
                    {
                        return i;
                    }
                }
            }
            return null;
        }
    }
}
