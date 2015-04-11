using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace VVVV.DX11
{
    public class DX11RenderSpace
    {
        public DX11RenderSpace()
        {
            this.View = Matrix.Identity;
            this.Projection = Matrix.Identity;
            this.NormalizedProjection = Matrix.Identity;
            this.Aspect = Matrix.Identity;
            this.Crop = Matrix.Identity;
            this.ViewProjection = Matrix.Identity;
        }


        public void Update(Matrix view, Matrix projection, Matrix aspect, Matrix crop)
        {
            this.View = view;
            this.Projection = projection;
            this.Aspect = aspect;
            this.Crop = crop;
            this.NormalizedProjection = this.Projection * Matrix.Invert(this.Aspect) * Matrix.Invert(this.Crop);
            this.ViewProjection = this.View * this.NormalizedProjection;
        }

        public void Update(Matrix view, Matrix projection)
        {
            this.Update(view, projection, Matrix.Identity, Matrix.Identity);
        }


        /// <summary>
        /// View Matrix
        /// </summary>
        public Matrix View { get; private set; }

        /// <summary>
        /// Projection Matrix
        /// </summary>
        public Matrix Projection { get; private set; }

        /// <summary>
        /// Aspect Ratio
        /// </summary>
        public Matrix Aspect { get; private set; }

        /// <summary>
        /// Crop Transform
        /// </summary>
        public Matrix Crop { get; private set; }

        /// <summary>
        /// Normalized projection matrix
        /// </summary>
        public Matrix NormalizedProjection { get; private set; }

        /// <summary>
        /// Normalized projection matrix
        /// </summary>
        public Matrix ViewProjection { get; private set; }


    }
}
