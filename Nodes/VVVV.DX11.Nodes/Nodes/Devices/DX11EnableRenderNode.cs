using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;


using FeralTic.DX11;
using FeralTic.DX11.Queries;

using VVVV.DX11.Lib.Devices;
using VVVV.DX11.Lib.RenderGraph;
using SlimDX.Direct3D11;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "EnableRender", Category = "DX11", Version = "", Author = "vux", Tags = "debug", AutoEvaluate = true)]
    public class DX11EnableRenderNode : IPluginEvaluate
    {
        [Input("Enabled", DefaultValue = 1)]
        protected ISpread<bool> FINEnabled;

        public void Evaluate(int SpreadMax)
        {
            DX11GlobalDevice.RenderManager.Enabled = this.FINEnabled[0];
        }
    }
}
