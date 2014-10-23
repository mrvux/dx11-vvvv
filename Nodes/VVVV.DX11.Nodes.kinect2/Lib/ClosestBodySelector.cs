using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K2Tools
{
    /// <summary>
    /// Selects closest body (as per lowest Z) from list
    /// </summary>
    public class ClosestBodySelector : IBodySelector
    {
        public Body Select(IEnumerable<Body> bodies)
        {
            Body result = null;
            float minZ = float.MaxValue;

            foreach (Body b in bodies)
            {
                var z = b.Joints[JointType.SpineBase].Position.Z;
                if (z < minZ)
                {
                    result = b;
                    minZ = z;
                }
            }
            return result;
        }
    }
}
