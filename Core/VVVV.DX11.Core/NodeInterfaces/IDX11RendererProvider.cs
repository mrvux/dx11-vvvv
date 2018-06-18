using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FeralTic.DX11;

namespace VVVV.DX11
{
    /// <summary>
    /// Renderer host allows to provide an additional render stage
    /// </summary>
    public interface IDX11RendererHost : IDX11ResourceHost
    {
        /// <summary>
        /// Indicates if node is enabled at all
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Render content
        /// </summary>
        void Render(DX11RenderContext context);
    }
}
