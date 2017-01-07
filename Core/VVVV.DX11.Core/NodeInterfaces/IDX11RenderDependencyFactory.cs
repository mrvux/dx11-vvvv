using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11
{
    public interface IDX11RenderDependencyFactory
    {
        void CreateDependency(IPin inputPin, IPin outputPin);
        void DeleteDependency(IPin inputPin);
    }
}
