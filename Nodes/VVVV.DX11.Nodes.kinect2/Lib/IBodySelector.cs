using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K2Tools
{
    /// <summary>
    /// Small interface to allow active body selection
    /// </summary>
    public interface IBodySelector
    {
        /// <summary>
        /// Selects a body from list
        /// </summary>
        /// <param name="bodies">Body list</param>
        /// <returns>Selected body</returns>
        Body Select(IEnumerable<Body> bodies);
    }
}
