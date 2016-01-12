using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11
{
    public interface IDX11LayerOrder
    {
        bool Enabled { get; }
        List<int> Reorder(DX11RenderSettings settings, List<DX11ObjectRenderSettings> objectSettings);
    }
}
