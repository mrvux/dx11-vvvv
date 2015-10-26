using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11
{
    /// <summary>
    /// This node updates all it's resources in one go, 
    /// so once first update been called mark other outputs as processed
    /// </summary>
    [Obsolete("This will be removed in next release, as it's never been used anyway")]
    public interface IDX11MultiResourceProvider : IDX11ResourceProvider
    {

    }
}
