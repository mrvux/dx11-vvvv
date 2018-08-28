using System.Collections.Generic;

namespace VVVV.DX11.Validators
{
    public class DX11WithinViewportValidator : IDX11ObjectValidator
    {
        private DX11RenderSettings settings;

        public bool Enabled { get; set; }

        public List<int> ViewPortIndices { get; set; }

        public void SetGlobalSettings(DX11RenderSettings settings)
        {
            this.settings = settings;

        }

        public bool Validate(DX11ObjectRenderSettings obj)
        {
            return ViewPortIndices[obj.DrawCallIndex % ViewPortIndices.Count] == settings.ViewportIndex;
        }

        public void Reset()
        {
        }
    }
}
