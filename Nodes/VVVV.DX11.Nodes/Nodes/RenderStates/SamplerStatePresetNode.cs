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
        [Input("Mode")]
        protected IDiffSpread<SamplerStatePreset> FMode;

        [Output("Sampler")]
        protected ISpread<SamplerDescription> FOutSampler;

        public void Evaluate(int SpreadMax)
        {
            if (this.FMode.IsChanged)
            {
                this.FOutSampler.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FOutSampler[i] = DX11SamplerStates.GetState(this.FMode[i]);
                }
            }
        }
    }
}


