using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using FeralTic.DX11;

namespace VVVV.DX11
{
    public delegate void DX11RenderRequestDelegate(IDX11ResourceDataRetriever sender, IPluginHost host);

    /// <summary>
    /// Interface for nodes which requires a back copy from gpu to cpu
    /// </summary>
    public interface IDX11ResourceDataRetriever
    {
        DX11RenderContext AssignedContext { get; set; }

        event DX11RenderRequestDelegate RenderRequest;
    }
}
