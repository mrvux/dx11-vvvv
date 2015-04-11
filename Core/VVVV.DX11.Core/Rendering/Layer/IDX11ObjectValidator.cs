using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11
{
    public interface IDX11ObjectValidator
    {
        void Reset();
        void SetGlobalSettings(DX11RenderSettings settings);
        bool Validate(DX11ObjectRenderSettings obj);
        bool Enabled { get; }
    }
}
