using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;

namespace VVVV.DX11
{
    /// <summary>
    /// Indicates that we are part of the render graph, but with no affiliated operations
    /// </summary>
    public interface IDX11RenderGraphPart
    {

    }

    /// <summary>
    /// Indicates that the node provides DX11 Resources
    /// </summary>
    public interface IDX11ResourceHost : IDX11RenderGraphPart
    {
        /// <summary>
        /// Updates resource (called by subgraph)
        /// </summary>
        /// <param name="OnDevice">DX11 Device</param>
        void Update(DX11RenderContext context);

        /// <summary>
        /// Destroys resource (called by subgraph)
        /// </summary>
        /// <param name="OnDevice">DX11 Device</param>
        /// <param name="force">True in case we need to kill resource (device/node disposed), false if safe to keep in memory</param>
        void Destroy(DX11RenderContext context, bool force);
    }
}
