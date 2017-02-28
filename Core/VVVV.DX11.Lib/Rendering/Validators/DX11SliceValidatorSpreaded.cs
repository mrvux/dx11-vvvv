using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11.Validators
{
    public class DX11SliceValidatorSpreaded : IDX11ObjectValidator
    {
        public bool Enabled { get; set; }
        private DX11RenderSettings settings;

        public List<int> Index = new List<int>();

        public void SetGlobalSettings(DX11RenderSettings settings)
        {
            this.settings = settings;

        }


        public bool Validate(DX11ObjectRenderSettings obj)
        {
            return this.Index.Contains(obj.DrawCallIndex);
        }

        public void Reset()
        {
        }
    }

}
