using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FeralTic.DX11;

namespace VVVV.DX11
{
    /// <summary>
    /// Renderer provider also needs to render as soon as update
    /// is fully satisfied.
    /// </summary>
    [Obsolete("Resource provider does access IPluginIO, which fails in case of multi core access, this will be removed in next release, use IDX11RendererHost instead")]
    public interface IDX11RendererProvider : IDX11ResourceProvider
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

    /// <summary>
    /// Renderer provider also needs to render as soon as update
    /// is fully satisfied.
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
