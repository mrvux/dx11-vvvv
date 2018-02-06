using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FeralTic.Core.Maths;
using SlimDX;

namespace VVVV.DX11.Validators
{
    public class WithinSphereValidator : IDX11ObjectValidator
    {
        public bool Enabled { get; set; }
        public BoundingSphere BoundingSphere;

        public void SetGlobalSettings(DX11RenderSettings settings)
        {

        }

        public bool Validate(DX11ObjectRenderSettings obj)
        {
            if (obj.Geometry.HasBoundingBox == false)
                return true;

            Matrix worldMatrix = obj.WorldTransform;
            BoundingBox boundingBox = obj.Geometry.BoundingBox;
            boundingBox.Maximum = Vector3.TransformCoordinate(boundingBox.Maximum, worldMatrix);
            boundingBox.Minimum = Vector3.TransformCoordinate(boundingBox.Minimum, worldMatrix);
            return BoundingSphere.Contains(this.BoundingSphere, boundingBox) != ContainmentType.Disjoint;
        }

        public void Reset()
        {
        }
    }
}
