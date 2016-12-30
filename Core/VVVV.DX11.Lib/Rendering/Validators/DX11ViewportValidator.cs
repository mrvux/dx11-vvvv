using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FeralTic.Core.Maths;

namespace VVVV.DX11.Validators
{
    public class DX11ViewportValidator : IDX11ObjectValidator
    {
        private DX11RenderSettings settings;

        public bool Enabled { get; set; }

        public int ViewPortCount { get; set; }

        public bool Cycling { get; set; }

        public void SetGlobalSettings(DX11RenderSettings settings)
        {
            this.settings = settings;

        }

        public bool Validate(DX11ObjectRenderSettings obj)
        {
            return this.Cycling ? (obj.DrawCallIndex % ViewPortCount == settings.ViewportIndex) : (obj.DrawCallIndex == settings.ViewportIndex);
        }

        public void Reset()
        {
        }
    }
}
