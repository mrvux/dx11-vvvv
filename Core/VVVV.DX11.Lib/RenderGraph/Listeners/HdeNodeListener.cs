using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2.Graph;
using VVVV.PluginInterfaces.V2;
using VVVV.Core;
using VVVV.Core.Logging;

namespace VVVV.DX11.Lib.RenderGraph.Listeners
{
    

    /// <summary>
    /// Recursive Node listener, sends events when it receives a node of interest
    /// </summary>
    public abstract class AbstractHdeNodeListener
    {
        protected IHDEHost hdehost;

        protected abstract bool ProcessAddedNode(INode2 node);
        protected abstract bool ProcessRemovedNode(INode2 node);
        protected abstract bool ProcessAddedPin(IPin2 pin,bool imediate);
        protected abstract bool ProcessRemovedPin(IPin2 pin);


        public AbstractHdeNodeListener(IHDEHost hde)
        {
            this.hdehost = hde;
            /*this.hdehost.RootNode.Added += OnNodeAdded;
            this.hdehost.RootNode.Removed += OnNodeRemoved;*/
            this.AddNode(this.hdehost.RootNode);
        }

        #region Node Events
        private void OnNodeAdded(IViewableCollection<INode2> collection, INode2 item)
        {
            this.AddNode(item);
        }

        private void OnNodeRemoved(IViewableCollection<INode2> collection, INode2 item)
        {
            this.RemoveNode(item);
        }
        #endregion

        //Add Remove Node processing

        #region Add Node
        private void AddNode(INode2 node)
        {
            //Add listener for patches, since they can have children
            if (node.HasPatch)
            {
                node.Added += OnNodeAdded;
                node.Removed += OnNodeRemoved;

                //Recursively add children
                foreach (INode2 child in node)
                {
                    this.AddNode(child);
                }
            }

            if (this.ProcessAddedNode(node))
            {
                node.Pins.Added += this.Pins_Added;
                node.Pins.Removed += this.Pins_Removed;
            }
        }

        private void Pins_Added(IViewableCollection<IPin2> collection, IPin2 item)
        {
            this.ProcessAddedPin(item,false);
        }

        private void Pins_Removed(IViewableCollection<IPin2> collection, IPin2 item)
        {
            this.ProcessRemovedPin(item);
        }
        #endregion

        #region Remove Node
        private void RemoveNode(INode2 node)
        {
            //Remove listeners if patch
            if (node.HasPatch)
            {
                node.Added -= OnNodeAdded;
                node.Removed -= OnNodeRemoved;

                //Remove all children
                foreach (INode2 child in node)
                {
                    this.RemoveNode(child);
                }
            }

            this.ProcessRemovedNode(node);
        }
        #endregion

    } 
}
