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
        public Matrix View;

        /// <summary>
        /// Projection Matrix
        /// </summary>
        public Matrix RawProjection;

        /// <summary>
        /// Projection Matrix (Normalized)
        /// </summary>
        public Matrix Projection;

        /// <summary>
        /// View Projection Matrix
        /// </summary>
        public Matrix ViewProjection;

        /// <summary>
        /// Aspect Ratio
        /// </summary>
        public Matrix Aspect;

        /// <summary>
        /// Crop Transform
        /// </summary>
        public Matrix Crop;

        public void ApplyTransforms(Matrix view,Matrix projection,Matrix aspect,Matrix crop)
        {
            this.View = view;
            this.RawProjection = projection;
            this.Aspect = Matrix.Invert(aspect);
            this.Crop = Matrix.Invert(crop);
            this.Projection = this.RawProjection * this.Aspect * this.Crop;
            this.ViewProjection = this.View * this.Projection;
        }
    }
}
