using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FeralTic.DX11;

namespace VVVV.DX11
{
    /// <summary>
    /// Render start point is used to nodes that will begin rendering (like windows or anything that needs presenting)
    /// </summary>
    public interface IDX11RenderStartPoint : IDX11RenderGraphPart
    {
        /// <summary>
        /// Gets render context 
        /// </summary>
        DX11RenderContext RenderContext { get; }
        /// <summary>
        /// Tells if our part is enabled
        /// </summary>
        bool Enabled { get; }
        /// <summary>
        /// Presents render
        /// </summary>
        void Present();
    }

    public interface IDX11RenderWindow : IDX11RenderStartPoint , IAttachableWindow
    {

    }
}
