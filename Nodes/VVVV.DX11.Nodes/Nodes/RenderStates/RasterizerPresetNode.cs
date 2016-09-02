using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Rasterizer", Category = "DX11.RenderState", Tags="fill, point, wireframe, solid", Author = "vux")]
    public class RasterizerPresetNode : BaseDX11RenderStateSimple
    {
        [ImportingConstructor()]
        public RasterizerPresetNode(IPluginHost host, IIOFactory iofactory) : base(host, iofactory) { }

        protected override DX11RenderState AssignPreset(string key, DX11RenderState statein)
        {
            statein.Rasterizer = DX11RasterizerStates.Instance.GetState(key);
            return statein;
        }

        protected override InputAttribute GetEnumPin()
        {
            InputAttribute attr = new InputAttribute("Mode");
            attr.EnumName = DX11RasterizerStates.Instance.EnumName;
            return attr;

        }
    }
}
