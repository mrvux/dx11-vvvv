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
    [PluginInfo(Name = "DepthStencil", Category = "DX11.RenderState", Author = "vux")]
    public class DepthStencilPresetNode : BaseDX11RenderStateSimple
    {
        [ImportingConstructor()]
        public DepthStencilPresetNode(IPluginHost host, IIOFactory iofactory) : base(host, iofactory) { }

        protected override DX11RenderState AssignPreset(string key, DX11RenderState statein)
        {
            statein.DepthStencil = DX11DepthStencilStates.Instance.GetState(key);
            return statein;
        }

        protected override InputAttribute GetEnumPin()
        {
            string[] enums = DX11DepthStencilStates.Instance.StateKeys;

            this.FHost.UpdateEnum(DX11DepthStencilStates.Instance.EnumName,enums[0],enums);

            InputAttribute attr = new InputAttribute("Mode");
            attr.EnumName = DX11DepthStencilStates.Instance.EnumName;
            attr.DefaultEnumEntry = enums[0];

            return attr;
            
        }
    }
}
