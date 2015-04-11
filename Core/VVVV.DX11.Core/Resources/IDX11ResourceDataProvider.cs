using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11
{
    /// <summary>
    /// Interface to provide for resource holding data per device
    /// </summary>
    public interface IDX11ResourceDataProvider
    {
        Dictionary<DX11RenderContext, IDX11Resource> Data { get; }
    }
}
