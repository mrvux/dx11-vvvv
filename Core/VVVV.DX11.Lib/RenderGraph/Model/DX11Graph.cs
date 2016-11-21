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
        private List<DX11Node> nodes = new List<DX11Node>();
        private List<DX11Node> renderwindows = new List<DX11Node>();
        private List<DX11Node> renderStartPoints = new List<DX11Node>();

        public DX11Graph()
        {

        }

        public List<DX11Node> Nodes
        {
            get { return this.nodes; }
        }

        public List<DX11Node> RenderWindows
        {
            get { return this.renderwindows; }
        }

        public List<DX11Node> RenderStartPoints
        {
            get { return this.renderStartPoints; }
        }

        public void AddNode(DX11Node node)
        {
            this.nodes.Add(node);
            if (node.Interfaces.IsRenderWindow)
            {
                this.renderwindows.Add(node);
            }
            if (node.Interfaces.IsRenderStartPoint)
            {
                this.renderStartPoints.Add(node);
            }
        }

        public void RemoveNode(DX11Node node)
        {
            this.nodes.Remove(node);
            this.renderwindows.Remove(node);
            this.renderStartPoints.Remove(node);
        }

        public DX11Node FindNode(INode2 hdenode)
        {
            foreach (DX11Node n in this.nodes)
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
            foreach (DX11Node n in this.nodes)
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
            foreach (DX11Node n in this.nodes)
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
            foreach (DX11Node n in this.nodes)
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
