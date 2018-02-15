using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Core.DirectWrite
{
    /// <summary>
    /// Interface to implement in order to apply style to a text layout
    /// </summary>
    public interface ITextStyler
    {
        /// <summary>
        /// Applies the style to a given text layout
        /// </summary>
        /// <param name="layout">Text layout to apply style to</param>
        void Apply(TextLayout layout);
    }
}
