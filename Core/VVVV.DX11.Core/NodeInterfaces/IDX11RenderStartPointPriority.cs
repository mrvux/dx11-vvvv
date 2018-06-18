using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.NodeInterfaces
{
    /// <summary>
    /// Combined with render start point, this allows to set a priority.
    /// </summary>
    public interface IDX11RenderStartPointPriority
    {
        /// <summary>
        /// Start point priorty
        /// </summary>
        RenderStartPointPriority Priority { get; }
    }
}
