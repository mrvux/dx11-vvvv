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
using VVVV.DX11.Internals.Helpers;
using VVVV.Utils.Win32;
using System.IO;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "AsTexture", Category = "DX11.Texture", Version = "2d Raw", Author = "vux", Help ="Creates a DirectX11 texture from raw pixel data (not from a file format")]
    public unsafe class DynamidRawTexture : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Import()]
        protected ILogger logger;

        [Input("Width", DefaultValue = 1,AutoValidate=false)]
        protected ISpread<int> FInWidth;

        [Input("Height", DefaultValue = 1, AutoValidate = false)]
        protected ISpread<int> FInHeight;

        [Input("Format", DefaultEnumEntry ="R8G8B8A8_UNrom", AutoValidate = false)]
        protected ISpread<SlimDX.DXGI.Format> FInFormat;

        [Input("Stride", DefaultValue = 0, AutoValidate = false)]
        protected ISpread<int> FInStride;

        [Input("Data", DefaultValue = 0, AutoValidate = false)]
        protected Pin<Stream> FInData;

        [Input("Read Location", DefaultValue = 0, AutoValidate = false)]
        protected ISpread<int> FInDataLocation;

        [Input("Apply", IsBang = true, DefaultValue = 1)]
        protected ISpread<bool> FApply;

        [Output("Texture Out")]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureOutput;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private bool FInvalidate;

        private Spread<byte> byteSpread = new Spread<byte>(1);

        public void Evaluate(int SpreadMax)
        {
            if (this.FApply[0])
            {
                this.FInWidth.Sync();
                this.FInHeight.Sync();
                this.FInFormat.Sync();
                this.FInStride.Sync();

                this.FInData.Sync();
                this.FInDataLocation.Sync();

                this.FInvalidate = true;
            }

            if (SpreadMax == 0)
            {
                if (this.FTextureOutput.SliceCount == 1)
                {
                    this.FTextureOutput.SafeDisposeAll();
                }
            }
            else
            {
                this.FTextureOutput.SliceCount = 1;
                if (this.FTextureOutput[0] == null)
                {
                    this.FTextureOutput[0] = new DX11Resource<DX11Texture2D>();
                }
            }
        }

        public unsafe void Update(DX11RenderContext context)
        {
            if (this.FTextureOutput.SliceCount == 0) { return; }

            if (this.FInvalidate)
            {

                int index = 0;
                var data = this.FInData[index];

                if (this.FTextureOutput[0].Contains(context))
                {
                    this.FTextureOutput[0].Dispose(context);
                }

                if (this.FInData.IsConnected && data != null)
                {
                    int width = this.FInWidth[index];
                    int height = this.FInHeight[index];
                    var fmt = this.FInFormat[index];

                    int pixelSize = DeviceFormatHelper.GetPixelSizeInBytes(fmt);

                    int stride = this.FInStride[index];
                    stride = stride <= 0 ? pixelSize * width : stride;

                    //Normally spread implementation, afaik , doesn't downsize the buffer
                    byteSpread.SliceCount = stride * pixelSize;

                    data.Position = this.FInDataLocation[0];
                    data.Read(byteSpread.Stream.Buffer, 0, stride * height);
                    data.Position = 0;

                    using (SlimDX.DataStream dataStream = new DataStream(byteSpread.Stream.Buffer, true, true))
                    {
                        DX11Texture2D texture = DX11Texture2D.CreateImmutable(context, width, height, fmt, stride, dataStream);
                        this.FTextureOutput[0][context] = texture;
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
