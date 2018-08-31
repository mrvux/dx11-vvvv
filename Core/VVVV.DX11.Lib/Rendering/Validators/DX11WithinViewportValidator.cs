using System.Collections.Generic;
using System.Linq;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Validators
{
    public class DX11WithinViewportValidator : IDX11ObjectValidator
    {
        private DX11RenderSettings settings;

        public bool Enabled { get; set; }

        public List<int> ViewPortIndices { get; set; } = new List<int>();

        public void SetGlobalSettings(DX11RenderSettings settings)
        {
            this.settings = settings;

        }

        public bool Validate(DX11ObjectRenderSettings obj)
        {
            if (ViewPortIndices.Count > 0)
                return ViewPortIndices[obj.DrawCallIndex % ViewPortIndices.Count] == settings.ViewportIndex;
            else return true;
        }

        public void Reset()
        {
        }
    }
}
