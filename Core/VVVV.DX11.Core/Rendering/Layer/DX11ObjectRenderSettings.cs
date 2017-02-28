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
        public int DrawCallIndex;

        /// <summary>
        /// World Transform
        /// </summary>
        public Matrix WorldTransform;

        /// <summary>
        /// Object bounding box transform
        /// </summary>
        public IDX11Geometry Geometry;

        /// <summary>
        /// Geometry is from layer
        /// </summary>
#pragma warning disable 0618
        [Obsolete("Will be replaced soon by new layout cache")]
        public bool GeometryFromLayer;
#pragma warning restore 0618

        public int IterationIndex;

        public int IterationCount;

        /// <summary>
        /// Object render state tag
        /// </summary>
        public object RenderStateTag;

    }
}
