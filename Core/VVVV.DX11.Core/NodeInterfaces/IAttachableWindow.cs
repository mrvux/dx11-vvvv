using FeralTic.DX11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11
{
    /// <summary>
    /// Attachable window interface allows you to provide your own form
    /// </summary>
    public interface IAttachableWindow
    {
        /// <summary>
        /// Attach a render context to the widow
        /// </summary>
        /// <param name="renderContext"></param>
        void AttachContext(DX11RenderContext renderContext);

        /// <summary>
        /// Gets window handle
        /// </summary>
        IntPtr WindowHandle { get; }
    }
}

