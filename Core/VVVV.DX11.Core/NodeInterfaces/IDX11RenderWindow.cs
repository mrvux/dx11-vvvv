using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FeralTic.DX11;

namespace VVVV.DX11
{
    public interface IAttachableWindow
    {
        void AttachContext(DX11RenderContext renderContext);
        IntPtr WindowHandle { get; }
    }

    public interface IDX11RenderStartPoint : IDX11RenderGraphPart
    {
        DX11RenderContext RenderContext { get; }
        bool Enabled { get; }
        void Present();
    }

    public interface IDX11RenderWindow : IDX11RenderStartPoint , IAttachableWindow
    {

    }
}
