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
    /// Indicates that the node provides DX11 Resources
    /// </summary>
    public interface IDX11ResourceProvider
    {
        /// <summary>
        /// Updates resource (called by subgraph)
        /// </summary>
        /// <param name="pin">Pin to update resource from</param>
        /// <param name="OnDevice">DX11 Device</param>
        void Update(IPluginIO pin, DX11RenderContext context);

        /// <summary>
        /// Destroys resource (called by subgraph)
        /// </summary>
        /// <param name="pin">Pin to destroy resource from</param>
        /// <param name="OnDevice">DX11 Device</param>
        /// <param name="force">True in case we need to kill resource (device/node disposed), false if safe to keep in memory</param>
        void Destroy(IPluginIO pin, DX11RenderContext context, bool force);
    }




}
