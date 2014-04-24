using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;
using VVVV.Utils.VColor;
using SlimDX;
using VVVV.PluginInterfaces.V1;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Sampler", Category = "DX11", Version = "Advanced", Author = "vux,tonfilm")]
    public class DX11SamplerStateNode : IPluginEvaluate
    {
        [Input("Address U", DefaultEnumEntry = "Wrap")]
        protected IDiffSpread<TextureAddressMode> FInAddressU;

        [Input("Address V", DefaultEnumEntry = "Wrap")]
        protected IDiffSpread<TextureAddressMode> FInAddressV;

        [Input("Address W", DefaultEnumEntry = "Wrap")]
        protected IDiffSpread<TextureAddressMode> FInAddressW;

        [Input("Border Color", DefaultColor = new double[] { 0, 0, 0, 1 })]
        protected IDiffSpread<RGBAColor> FInBorderColor;

        [Input("Comparison", DefaultEnumEntry = "Always")]
        protected IDiffSpread<Comparison> FInComparison;

        [Input("Filter Mode", DefaultEnumEntry = "MinMagMipLinear")]
        protected IDiffSpread<Filter> FInFilterMode;

        [Input("Maximum Anisotropy", DefaultValue = 1)]
        protected IDiffSpread<int> FInMaximumAnisotropy;

        [Input("Minimum Lod", DefaultValue = float.MinValue)]
        protected IDiffSpread<float> FInMinimumLod;

        [Input("Maximum Lod", DefaultValue = float.MaxValue)]
        protected IDiffSpread<float> FInMaximumLod;

        [Input("Mip Lod Bias", DefaultValue = 0)]
        protected IDiffSpread<float> FInMipLodBias;

        [Output("Sampler")]
        protected ISpread<SamplerDescription> FOutSampler;


        public void Evaluate(int SpreadMax)
        {


            if (this.FInAddressU.IsChanged
                || this.FInAddressV.IsChanged
                || this.FInAddressW.IsChanged
                || this.FInBorderColor.IsChanged
                || this.FInComparison.IsChanged
                || this.FInFilterMode.IsChanged
                || this.FInMaximumAnisotropy.IsChanged
                || this.FInMaximumLod.IsChanged
                || this.FInMinimumLod.IsChanged
                || this.FInMipLodBias.IsChanged)
            {
                this.FOutSampler.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    RGBAColor c = this.FInBorderColor[i];

                    Color4 col = new Color4((float)c.R, (float)c.G, (float)c.B, (float)c.A);
                    SamplerDescription sampler = new SamplerDescription()
                    {
                        AddressU = this.FInAddressU[i],
                        AddressV = this.FInAddressV[i],
                        AddressW = this.FInAddressW[i],
                        BorderColor = col,
                        ComparisonFunction = this.FInComparison[i],
                        Filter = this.FInFilterMode[i],
                        MaximumAnisotropy = this.FInMaximumAnisotropy[i],
                        MaximumLod = this.FInMaximumLod[i],
                        MinimumLod = this.FInMinimumLod[i],
                        MipLodBias = this.FInMipLodBias[i]
                    };

                    this.FOutSampler[i] = sampler;
                }
            }
        }
    }
}


