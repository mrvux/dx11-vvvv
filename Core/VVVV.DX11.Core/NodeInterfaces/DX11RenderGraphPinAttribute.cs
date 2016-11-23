using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.NodeInterfaces
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DX11RenderGraphPinAttribute : Attribute
    {
    }
}
