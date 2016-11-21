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
using VVVV.Core.Logging;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "DynamicTexture", Category = "DX11.Texture", Version = "2d", Author = "vux")]
    public unsafe class DynamicTexture2DNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Import()]
        protected ILogger logger;

        [Input("Width", DefaultValue = 1,AutoValidate=false)]
        protected ISpread<int> FInWidth;

        [Input("Height", DefaultValue = 1, AutoValidate = false)]
        protected ISpread<int> FInHeight;

        [Input("Channel Count", DefaultValue = 1, AutoValidate = false, MinValue=1,MaxValue=4)]
        protected ISpread<int> FInChannels;

        [Input("Data", DefaultValue = 0, AutoValidate = false)]
        protected ISpread<float> FInData;

        [Input("Apply", IsBang = true, DefaultValue = 1)]
        protected ISpread<bool> FApply;

        [Config("Suppress Warning", DefaultValue = 0)]
        protected ISpread<bool> FSuppressWarning;

        [Output("Texture Out")]
        protected Pin<DX11Resource<DX11DynamicTexture2D>> FTextureOutput;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private bool FInvalidate;

        private float[] data = new float[0];

        public void Evaluate(int SpreadMax)
        {
            if (this.FApply[0])
            {
                this.FInChannels.Sync();
                this.FInData.Sync();
                this.FInHeight.Sync();
                this.FInWidth.Sync();
                this.FInvalidate = true;
                if (this.FInChannels[0] == 3 && FSuppressWarning[0] == false)
                {
                    logger.Log(LogType.Warning, "Using 3 channels texture format, samplers are not allowed in this case, use load only");
                }
            }

            if (this.FInWidth.SliceCount == 0
                || this.FInHeight.SliceCount == 0
                || this.FInData.SliceCount == 0
                || this.FInChannels.SliceCount == 0)
            {
                if (this.FTextureOutput.SliceCount == 1)
                {
                    if (this.FTextureOutput[0] != null) { this.FTextureOutput[0].Dispose(); }
                    this.FTextureOutput.SliceCount = 0;
                }
            }
            else
            {
                this.FTextureOutput.SliceCount = 1;
                if (this.FTextureOutput[0] == null) { this.FTextureOutput[0] = new DX11Resource<DX11DynamicTexture2D>(); }
            }
        }

        public unsafe void Update(DX11RenderContext context)
        {
            if (this.FTextureOutput.SliceCount == 0) { return; }

            if (this.FInvalidate || ! this.FTextureOutput[0].Contains(context))
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

                Texture2DDescription desc;

                if (this.FTextureOutput[0].Contains(context))
                {
                    desc = this.FTextureOutput[0][context].Resource.Description;

                    if (desc.Width != this.FInWidth[0] || desc.Height != this.FInHeight[0] || desc.Format != fmt)
                    {
                        this.FTextureOutput[0].Dispose(context);
                        this.FTextureOutput[0][context] = new DX11DynamicTexture2D(context, this.FInWidth[0], this.FInHeight[0], fmt);
                    }
                }
                else
                {
                    this.FTextureOutput[0][context] = new DX11DynamicTexture2D(context, this.FInWidth[0], this.FInHeight[0], fmt);
                    #if DEBUG
                    this.FTextureOutput[0][context].Resource.DebugName = "DynamicTexture";
                    #endif
                }

                desc = this.FTextureOutput[0][context].Resource.Description;

                int chans = this.FInChannels[0];
                chans = Math.Min(chans, 4);
                chans = Math.Max(chans, 1);

                if (data.Length != desc.Width * desc.Height * chans)
                {
                    data = new float[desc.Width * desc.Height * chans];
                }

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = this.FInData[i];
                }

                int stride = chans * 4;
                var t = this.FTextureOutput[0][context];
                fixed (float* fptr = & data[0])
                {
                    IntPtr ptr = new IntPtr(fptr);
                    if (t.GetRowPitch() == desc.Width * stride)
                    {
                        t.WriteData(ptr, desc.Width * desc.Height * stride);
                    }
                    else
                    {
                        t.WriteDataPitch(ptr, desc.Width * desc.Height * stride, stride);
                    }
                }
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
