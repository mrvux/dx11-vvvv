using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FeralTic.DX11;

namespace VVVV.DX11
{
    public interface IDX11RenderWindow
    {
        DX11RenderContext RenderContext { get; set; }
        IntPtr WindowHandle { get; }
        bool IsVisible { get; }
        void Present();
    }
}
