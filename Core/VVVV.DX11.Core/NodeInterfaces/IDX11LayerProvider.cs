using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;

namespace VVVV.DX11
{
    /// <summary>
    /// Layer need to provide additional interface
    /// </summary>
    [Obsolete("Resource provider does access IPluginIO, which fails in case of multi core access, this will be removed in next release, use IDX11LayerHost instead")]
    public interface IDX11LayerProvider : IDX11ResourceProvider
    {
        
    }

    /// <summary>
    /// Layer need to provide additional interface
    /// </summary>
    public interface IDX11LayerHost : IDX11ResourceHost
    {

    }
}
