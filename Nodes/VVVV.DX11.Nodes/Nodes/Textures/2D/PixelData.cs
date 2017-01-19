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

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "PixelData", Category = "DX11.Texture", Version = "2d", Author = "vux", Credits = "vux")]
    public class TexturePixelDataNode : IPluginEvaluate, IDX11ResourceDataRetriever, IPartImportsSatisfiedNotification
    {
        [Input("Texture In", IsSingle =true, AutoValidate =false)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureIn;

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

        Stream lastStream;

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
                        if (this.lastStream != null)
                        {
                            this.lastStream.Dispose();
                            this.lastStream = null;
                        }

                        var texture = this.FTextureIn[0][context];
                        var staging = new DX11StagingTexture2D(context, texture.Width, texture.Height, texture.Format);
                        staging.CopyFrom(texture);

                        var db = staging.LockForRead();

                        strideOut[0] = db.RowPitch;

                        MemoryStream ms = new MemoryStream((int)db.Data.Length);
                        db.Data.CopyTo(ms);
                        ms.Position = 0;

                        staging.UnLock();
                        staging.Dispose();

                        FStreamOut[0] = ms;
                        FStreamOut.Flush(true);
                        this.lastStream = ms;
                    }
                }
            }
        }

        #endregion
    }
}
