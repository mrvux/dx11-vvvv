using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FeralTic.Core.Maths;

namespace VVVV.DX11.Validators
{
    public class DX11FrustrumValidator : IDX11ObjectValidator
    {
        private DX11RenderSettings settings;
        private Frustrum frustrum = new Frustrum();

        public int Passed { get; set; }
        public int Failed { get; set; }
        public bool Enabled { get; set; }

        public void SetGlobalSettings(DX11RenderSettings settings)
        {
            this.settings = settings;
            this.frustrum.Initialize(settings.View, settings.Projection);
        }

        public bool Validate(DX11ObjectRenderSettings obj)
        {
            bool res = this.frustrum.Contains(obj.Geometry.BoundingBox, obj.WorldTransform);
            if (res) { Passed++; } else { Failed++; }
            return res;
        }

        public void Reset()
        {
            this.Passed = 0;
            this.Failed = 0;
        }
    }
}
