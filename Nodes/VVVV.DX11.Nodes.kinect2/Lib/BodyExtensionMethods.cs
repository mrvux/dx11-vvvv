using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K2Tools
{
    /// <summary>
    /// List of extension methods for kinect2 body
    /// </summary>
    public static class BodyExtensionMethods
    {
        public static IEnumerable<Body> GetTrackedBodies(this Body[] bodies)
        {
            return bodies.Where(b => b.IsTracked);
        }
    }
}
