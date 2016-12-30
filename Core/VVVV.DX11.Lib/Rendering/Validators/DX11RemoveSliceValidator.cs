using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11.Validators
{
    public class DX11RemoveSliceValidator : IDX11ObjectValidator
    {
        public bool Enabled { get; set; }
        private DX11RenderSettings settings;

        public int Index { get; set; }

        public void SetGlobalSettings(DX11RenderSettings settings)
        {
            this.settings = settings;
        }


        public bool Validate(DX11ObjectRenderSettings obj)
        {
            return obj.DrawCallIndex != this.Index;
        }

        public void Reset()
        {
        }
    }

}
