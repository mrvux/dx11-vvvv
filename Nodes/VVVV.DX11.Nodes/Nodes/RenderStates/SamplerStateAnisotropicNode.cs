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
    [PluginInfo(Name = "Anisotropic", Category = "DX11", Version = "Sampler", Author = "vux")]
    public class DX11SamplerStateAnisotropicNode : IPluginEvaluate
    {
        [Input("Address Mode", DefaultEnumEntry = "Wrap")]
        protected IDiffSpread<TextureAddressMode> FInAddress;

        [Input("Maximum Anisotropy", DefaultValue = 1, MinValue =0, MaxValue =16)]
        protected IDiffSpread<int> FInMaximumAnisotropy;

        [Input("Border Color", DefaultColor = new double[] { 0, 0, 0, 1 })]
        protected IDiffSpread<Color4> FInBorderColor;

        [Output("Sampler")]
        protected ISpread<SamplerDescription> FOutSampler;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInAddress.IsChanged || this.FInMaximumAnisotropy.IsChanged
                || this.FInBorderColor.IsChanged)
            {
                this.FOutSampler.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FOutSampler[i] = new SamplerDescription()
                    {
                        AddressU = this.FInAddress[i],
                        AddressV = this.FInAddress[i],
                        AddressW = this.FInAddress[i],
                        BorderColor = this.FInBorderColor[i],
                        ComparisonFunction = Comparison.Always,
                        Filter = Filter.Anisotropic,
                        MaximumAnisotropy = this.FInMaximumAnisotropy[i] < 0 ? 0 : this.FInMaximumAnisotropy[i] > 16 ? 16 : this.FInMaximumAnisotropy[i],
                        MaximumLod = float.MaxValue,
                        MinimumLod = float.MinValue,
                        MipLodBias = 0
                    };
                }
            }
        }
    }
}


