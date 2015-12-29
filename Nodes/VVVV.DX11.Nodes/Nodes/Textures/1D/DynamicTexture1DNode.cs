using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using SlimDX;
using SlimDX.Direct3D11;

using FeralTic.DX11.Resources;
using FeralTic.DX11;
using VVVV.Core.Logging;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "DynamicTexture", Category = "DX11.Texture", Version = "1d", Author = "vux")]
    public class DynamicTexture1DNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Import()]
        protected ILogger logger;

        [Config("Suppress Warning", DefaultValue = 0)]
        protected ISpread<bool> FSuppressWarning;


        [Input("Width", DefaultValue=1)]
        protected ISpread<int> FInWidth;

        [Input("Channel Count", DefaultValue = 1,MinValue=1,MaxValue=4)]
        protected ISpread<int> FInChannels;

        [Input("Data", DefaultValue = 0)]
        protected ISpread<float> FInData;

        [Input("Apply",IsBang=true, DefaultValue = 1)]
        protected ISpread<bool> FApply;

        [Output("Texture Out")]
        protected Pin<DX11Resource<DX11DynamicTexture1D>> FTextureOutput;

        private bool FInvalidate;

        public void Evaluate(int SpreadMax)
        {
            this.FTextureOutput.SliceCount = 1;
            this.FInvalidate = false;

            if (this.FTextureOutput[0] == null) { this.FTextureOutput[0] = new DX11Resource<DX11DynamicTexture1D>(); }

            if (this.FApply[0])
            {
                this.FInvalidate = true;
                if (this.FInChannels[0] == 3 && FSuppressWarning[0] == false)
                {
                    logger.Log(LogType.Warning, "Using 3 channels texture format, samplers are not allowed in this case, use load only");
                }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInvalidate)
            {
                SlimDX.DXGI.Format fmt;
                switch (this.FInChannels[0])
                {
                    case 1:
                        fmt = SlimDX.DXGI.Format.R32_Float;
                        break;
                    case 2:
                        fmt = SlimDX.DXGI.Format.R32G32_Float;
                        break;
                    case 3:
                        fmt = SlimDX.DXGI.Format.R32G32B32_Float;
                        break;
                    case 4:
                        fmt = SlimDX.DXGI.Format.R32G32B32A32_Float;
                        break;
                    default:
                        fmt = SlimDX.DXGI.Format.R32_Float;
                        break;
                }



                if (this.FTextureOutput[0].Contains(context))
                {
                    Texture1DDescription td = this.FTextureOutput[0][context].Resource.Description;


                    if (td.Width != this.FInWidth[0] || td.Format != fmt)
                    {
                        this.FTextureOutput[0].Dispose(context);
                        this.FTextureOutput[0][context] = new DX11DynamicTexture1D(context, this.FInWidth[0], fmt);
                    }
                }
                else
                {
                    this.FTextureOutput[0][context] = new DX11DynamicTexture1D(context, this.FInWidth[0], fmt);
                }

                DX11DynamicTexture1D tex = this.FTextureOutput[0][context];

                int chans = this.FInChannels[0];
                chans = Math.Min(chans, 4);
                chans = Math.Max(chans, 1);

                float[] data = new float[this.FInWidth[0] * chans];

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = this.FInData[i % data.Length];
                }
                tex.WriteData(data);           
            }
        }

        public void Destroy(DX11RenderContext OnDevice, bool force)
        {
        }

        #region IDisposable Members
        public void Dispose()
        {
            
        }
        #endregion
    }
}
