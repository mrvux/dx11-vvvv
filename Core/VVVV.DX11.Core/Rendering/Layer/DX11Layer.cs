using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11
{
    public delegate void RenderDelegate<T>(IPluginIO pin, DX11RenderContext context, T settings);

    public class DX11BaseLayer<T> : IDX11Resource
    {
        public RenderDelegate<T> Render;

        public bool PostUpdate
        {
            get { return true; }
        }

        public void Dispose()
        {
            
        }
    }

    /// <summary>
    /// DX11 Layer provide simple interface to tell which pin they need
    /// </summary>
    public class DX11Layer : DX11BaseLayer<DX11RenderSettings>
    {
    }
}
