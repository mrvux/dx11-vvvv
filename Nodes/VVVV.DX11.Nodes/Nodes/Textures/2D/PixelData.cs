using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using SlimDX.Direct3D11;

using VVVV.Core.Logging;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using System.IO;
using SlimDX;
using System.Runtime.InteropServices;
using FeralTic.DX11.Utils;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "PixelData", Category = "DX11.Texture", Version = "2d", Author = "vux", Credits = "vux")]
    public unsafe class TexturePixelDataNode : IPluginEvaluate, IDX11ResourceDataRetriever, IPartImportsSatisfiedNotification
    {
        private FormatHelper formatHelper = FormatHelper.Instance;

        [DllImport("msvcrt.dll", SetLastError = false)]
        static extern IntPtr memcpy(byte* dest, byte* src, int count);

        [Input("Texture In", IsSingle =true, AutoValidate =false)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureIn;

        [Input("Apply Stride")]
        protected ISpread<bool> FApplyStride;

        [Input("Read", IsBang = true)]
        protected ISpread<bool> FRead;

        [Output("Output")]
        public ISpread<Stream> FStreamOut;

        [Output("Output Stride")]
        public ISpread<int> strideOut;

        [Output("Valid")]
        protected ISpread<bool> FOutValid;

        [Import()]
        protected IPluginHost FHost;

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;

        //called when all inputs and outputs defined above are assigned from the host
        public void OnImportsSatisfied()
        {
            //start with an empty stream output
            FStreamOut.SliceCount = 0;
        }

        DataStream lastStream;
        byte[] binter = new byte[0];

        #region IPluginEvaluate Members

        public void Evaluate(int SpreadMax)
        {
            this.FOutValid.SliceCount = 1;
            this.FStreamOut.SliceCount = 1;

            if (this.FRead[0])
            {
                this.FTextureIn.Sync();
                if (this.FTextureIn.IsConnected)
                {
                    if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                    if (this.AssignedContext == null) { this.FOutValid.SliceCount = 0; return; }

                    var context = this.AssignedContext;



                    if (this.FTextureIn[0].Contains(this.AssignedContext))
                    {
                        

                        var texture = this.FTextureIn[0][context];
                        var staging = new DX11StagingTexture2D(context, texture.Width, texture.Height, texture.Format);
                        staging.CopyFrom(texture);

                        var db = staging.LockForRead();

                        strideOut[0] = db.RowPitch;

                        if (this.lastStream != null)
                        {
                            if (this.lastStream.Length != db.Data.Length)
                            {
                                this.lastStream.Dispose();
                                this.lastStream = null;
                            }
                        }

                        if (this.lastStream == null)
                        {
                            this.lastStream = new DataStream((int)db.Data.Length, true, true);
                        }

                        this.lastStream.Position = 0;

                        int pixelStride = formatHelper.GetSize(texture.Format);
                        int dstStride = pixelStride * texture.Width;

                        if (FApplyStride[0])
                        {
                            if (this.binter.Length != db.RowPitch)
                            {
                                this.binter = new byte[db.RowPitch];
                            }

                            byte* destPointer = (byte*)this.lastStream.DataPointer.ToPointer();
                            byte* srcPointer = (byte*)db.Data.DataPointer.ToPointer();

                            for (int i = 0; i < texture.Height; i++)
                            {
                                memcpy(destPointer, srcPointer, dstStride);
                                destPointer += dstStride;
                                srcPointer += db.RowPitch;
                            }
                        }
                        else
                        {
                            db.Data.CopyTo(this.lastStream);
                        }
                        this.lastStream.Position = 0;

                        staging.UnLock();
                        staging.Dispose();

                        FStreamOut[0] = this.lastStream;
                        FStreamOut.Flush(true);
                        this.FOutValid[0] = true;
                    }
                    else
                    {
                        this.FOutValid[0] = false;
                    }
                }
                else
                {
                    this.FOutValid[0] = false;
                }
            }
        }

        #endregion
    }
}
