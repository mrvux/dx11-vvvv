using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using FeralTic.Resources.Geometry;
using FeralTic.DX11.Resources;

namespace VVVV.DX11
{
    /// <summary>
    /// Per object render settings
    /// </summary>
    public class DX11ObjectRenderSettings
    {
        /// <summary>
        /// Draw Call Index (per shader)
        /// </summary>
        public int DrawCallIndex { get; set; }

        /// <summary>
        /// World Transform
        /// </summary>
        public Matrix WorldTransform { get; set; }

        /// <summary>
        /// Object bounding box transform
        /// </summary>
        public IDX11Geometry Geometry { get; set; }

        public int IterationIndex { get; set; }

        public int IterationCount { get; set; }

    }
}
