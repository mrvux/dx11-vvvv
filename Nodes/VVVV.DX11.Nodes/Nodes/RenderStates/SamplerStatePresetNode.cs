using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;
using VVVV.Utils.VColor;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Sampler", Category = "DX11", Version = "", Author = "vux")]
    public class DX11SamplerStatePresetNode : IPluginEvaluate
    {
        protected IDiffSpread<EnumEntry> FInPreset; 

        [Output("Sampler")]
        protected ISpread<SamplerDescription> FOutSampler;

        [ImportingConstructor()]
        public DX11SamplerStatePresetNode(IPluginHost host, IIOFactory iofactory)
        {
            string[] enums = DX11SamplerStates.Instance.StateKeys;

            host.UpdateEnum(DX11SamplerStates.Instance.EnumName, enums[0], enums);

            InputAttribute attr = new InputAttribute("Mode");
            attr.EnumName = DX11SamplerStates.Instance.EnumName;
            attr.DefaultEnumEntry = enums[0];
            this.FInPreset = iofactory.CreateDiffSpread<EnumEntry>(attr);
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInPreset.IsChanged)
            {
                this.FOutSampler.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FOutSampler[i] = DX11SamplerStates.Instance.GetState(this.FInPreset[i].Name);
                }
            }
        }
    }
}


