using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11
{
    /// <summary>
    /// This node is allowed to cut graph if not enabled, 
    /// please note that update can still be called on it, so need to take care of it
    /// </summary>
    public interface IDX11UpdateBlocker
    {
        bool Enabled { get; }
    }
}
