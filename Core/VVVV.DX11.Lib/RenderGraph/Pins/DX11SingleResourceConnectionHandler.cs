using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.DX11;

namespace VVVV.DX11.Lib.RenderGraph.Pins
{
    public class DX11SingleResourceConnectionHandler : IConnectionHandler
    {
        private readonly INodeOut nodeOut;

        public DX11SingleResourceConnectionHandler(INodeOut nodeOut)
        {
            this.nodeOut = nodeOut;
        }

        public bool Accepts(object source, object sink)
        {
            if (this.nodeOut.IsConnected)
                return false;

            Type[] sourcetype = source.GetType().GetGenericArguments();
            Type[] sinktype = sink.GetType().GetGenericArguments();

            if (sourcetype.Length == 2 && sinktype.Length == 2)
            {
                //return sinktype[].GetGenericArguments()[0].IsAssignableFrom(sourcetype[0].GetGenericArguments()[0]);
                if (!sourcetype[0].IsGenericType || !sinktype[0].IsGenericType) { return false; }

                if (sourcetype[0].GetGenericTypeDefinition() == typeof(DX11Resource<>) && sinktype[0].GetGenericTypeDefinition() == typeof(DX11Resource<>))
                {
                    return sinktype[0].GetGenericArguments()[0].IsAssignableFrom(sourcetype[0].GetGenericArguments()[0]);
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

        public string GetFriendlyNameForSink(object sink)
        {
            var sinkDataType = sink.GetType().GetGenericArguments()[1];
            return string.Format(" [ Needs: {0} ]", sinkDataType.FullName);          
        }

        public string GetFriendlyNameForSource(object source)
        {
            var sourceDataType = source.GetType().GetGenericArguments()[1];
            return string.Format(" [ Supports: {0} ]", sourceDataType.FullName);
        }
    }

}
