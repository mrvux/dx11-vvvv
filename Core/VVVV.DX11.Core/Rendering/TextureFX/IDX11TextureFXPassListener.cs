using FeralTic.DX11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.Rendering.TextureFX
{
    /// <summary>
    /// Interface to implement to apply pre/post pass global actions on texture fx
    /// </summary>
    public interface  IDX11TextureFXPassListener
    {
        /// <summary>
        /// Called just before starting a new pass
        /// </summary>
        /// <param name="renderContext">Render context</param>
        /// <param name="index">Pass index</param>
        void OnBeginPass(DX11RenderContext renderContext, int index);

        /// <summary>
        /// Called once a pass is finished
        /// </summary>
        /// <param name="renderContext">Render context</param>
        /// <param name="index">Pass index</param>
        void OnEndPass(DX11RenderContext renderContext, int index);
    }
}
