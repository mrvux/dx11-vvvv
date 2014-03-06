using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

namespace VVVV.DX11
{
    public partial class DX11RenderSettings
    {
        
        /// <summary>
        /// View Matrix
        /// </summary>
        public Matrix View { get; set; }

        /// <summary>
        /// Projection Matrix
        /// </summary>
        public Matrix Projection { get; set; }

        /// <summary>
        /// View Projection Matrix
        /// </summary>
        public Matrix ViewProjection { get; set; }

        /// <summary>
        /// Aspect Ratio
        /// </summary>
        public Matrix Aspect { get; set; }

        /// <summary>
        /// Crop Transform
        /// </summary>
        public Matrix Crop { get; set; }
    }
}
