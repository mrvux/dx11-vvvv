using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11
{
    /// <summary>
    /// Render start point priority allows to specify how fast we want to allow a start point to be started with.
    /// For example, start point that perform resource loading should get a high priority.
    /// Priority is normally an int, but here we provide some presets
    /// </summary>
    public enum RenderStartPointPriority : int
    {
        Low = 20,
        Normal = 10,
        High = 0
    }
}
