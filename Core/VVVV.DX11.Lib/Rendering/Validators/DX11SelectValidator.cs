using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Validators
{
    public class DX11SelectValidator : IDX11ObjectValidator
    {
        public bool Enabled { get; set; }
        private DX11RenderSettings settings;

        public ISpread<bool> Selection { get; set; }

        public void SetGlobalSettings(DX11RenderSettings settings)
        {
            this.settings = settings;
        }


        public bool Validate(DX11ObjectRenderSettings obj)
        {
            return this.Selection[obj.DrawCallIndex];
        }

        public void Reset()
        {
        }
    }
}
