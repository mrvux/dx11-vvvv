using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using SlimDX.Direct3D11;
using SlimDX;

using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "DynamicTexture", Category = "DX11.Texture", Version = "1d Color", Author = "vux")]
    public unsafe class DynamicTexture1DColorNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Width", DefaultValue = 1,AutoValidate=false)]
        protected ISpread<int> FInWidth;

        [Input("Data", DefaultValue = 0, AutoValidate = false)]
        protected ISpread<Color4> FInData;

        [Input("Apply", IsBang = true, DefaultValue = 1)]
        protected ISpread<bool> FApply;

        [Output("Texture Out", IsSingle=true)]
        protected Pin<DX11Resource<DX11DynamicTexture1D>> FTextureOutput;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private bool FInvalidate;

        private Color4[] data = new Color4[0];

        public void Evaluate(int SpreadMax)
        {
            if (this.FTextureOutput[0] == null) { this.FTextureOutput[0] = new DX11Resource<DX11DynamicTexture1D>(); }
            this.FValid.SliceCount = 1;

            if (this.FApply[0])
            {
                this.FInData.Sync();
                this.FInWidth.Sync();
                this.FInvalidate = true;
            }
        }

        public unsafe void Update(DX11RenderContext context)
        {
            if (this.FInvalidate || ! this.FTextureOutput[0].Contains(context))
            {

                SlimDX.DXGI.Format fmt = SlimDX.DXGI.Format.R32G32B32A32_Float;

                Texture1DDescription desc;

                if (this.FTextureOutput[0].Contains(context))
                {
                    desc = this.FTextureOutput[0][context].Resource.Description;

                    if (desc.Width != this.FInWidth[0] || desc.Format != fmt)
                    {
                        this.FTextureOutput[0].Dispose(context);
                        this.FTextureOutput[0][context] = new DX11DynamicTexture1D(context, this.FInWidth[0], fmt);
                    }
                }
                else
                {
                    this.FTextureOutput[0][context] = new DX11DynamicTexture1D(context, this.FInWidth[0], fmt);
                }

                desc = this.FTextureOutput[0][context].Resource.Description;

                if (data.Length != desc.Width)
                {
                    data = new Color4[desc.Width];
                }

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = this.FInData[i];
                }

                var t = this.FTextureOutput[0][context];
                t.WriteData(data);
                this.FInvalidate = false;
            }

        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FTextureOutput[0].Dispose(context);
        }


        #region IDisposable Members
        public void Dispose()
        {
            if (this.FTextureOutput.SliceCount > 0)
            {
                if (this.FTextureOutput[0] != null)
                {
                    this.FTextureOutput[0].Dispose();
                }
            }
        }
        #endregion
    }
}
