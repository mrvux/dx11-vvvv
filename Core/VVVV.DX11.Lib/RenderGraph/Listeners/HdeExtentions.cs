using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Factories;
using VVVV.Hosting.IO;

namespace VVVV
{
    public static class HdeExtentions
    {

        #region INode Assignable from
        public static bool IsNodeAssignableFrom<T>(this INode2 node)
        {
            if (node.NodeInfo.Type == NodeType.Dynamic || node.NodeInfo.Type == NodeType.Plugin)
            {
                IPluginHost host = (IPluginHost)node.InternalCOMInterf;


                IInternalPluginHost iip = (IInternalPluginHost)host;

                try
                {
                    if (iip.Plugin is PluginContainer)
                    {
                        PluginContainer plugin = (PluginContainer)iip.Plugin;
                        return typeof(T).IsAssignableFrom(plugin.PluginBase.GetType());
                    }
                    else
                    {
                        return typeof(T).IsAssignableFrom(iip.Plugin.GetType());
                    }
                }
                catch
                {
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine("Error checking node:" + node.NodeInfo.Name);
                    #endif
                    return false;
                }

            }
            else
            {
                return false;
            }
        }


        public static bool IsNodeAssignableFrom<T>(this INode node)
        {
            if (node.GetNodeInfo().Type == NodeType.Dynamic || node.GetNodeInfo().Type == NodeType.Plugin)
            {
                IPluginHost host = (IPluginHost)node;


                IInternalPluginHost iip = (IInternalPluginHost)host;

                if (iip.Plugin != null)
                {
                    if (iip.Plugin is PluginContainer)
                    {
                        PluginContainer plugin = (PluginContainer)iip.Plugin;
                        return typeof(T).IsAssignableFrom(plugin.PluginBase.GetType());
                    }
                    else
                    {
                        return typeof(T).IsAssignableFrom(iip.Plugin.GetType());
                    }
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }
        #endregion


        #region Find Hde Pin
        public static IPin FindHdePinByName(this INode node, string name)
        {
            foreach (IPin pin in node.GetPins())
            {
                if (pin.Name == name)
                {
                    return pin;
                }
            }
            return null;
        }
        #endregion





    }
}
