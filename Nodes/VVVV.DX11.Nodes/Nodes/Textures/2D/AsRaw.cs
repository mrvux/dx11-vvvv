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

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "AsRaw", Category = "DX11.Texture", Version = "2d", Author = "sebl", Credits = "vux", AutoEvaluate = true)]
    public class AsRawTextureNode : IPluginEvaluate, IDX11ResourceDataRetriever, IPartImportsSatisfiedNotification
    {
        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureIn;

        [Input("Format")]
        protected ISpread<ImageFileFormat> FInFormat;

        [Input("Read", IsBang = true)]
        protected ISpread<bool> FRead;

        [Output("Output")]
        public ISpread<Stream> FStreamOut;

        [Output("Valid")]
        protected ISpread<bool> FOutValid;

        [Import()]
        protected IPluginHost FHost;

        [Import()]
        protected ILogger FLogger;

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

        //Stream tempStream;

        #region IPluginEvaluate Members

        public void Evaluate(int SpreadMax)
        {
            this.FOutValid.SliceCount = SpreadMax;

            if (this.FTextureIn.IsConnected)
            {
                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null) { this.FOutValid.SliceCount = 0; return; }
                //Do NOT cache this, assignment done by the host

                FStreamOut.ResizeAndDispose(SpreadMax, () => new MemoryStream());

                for (int i = 0; i < SpreadMax; i++)
                {
                    if (this.FTextureIn[i].Contains(this.AssignedContext) && this.FRead[i])
                    {
                        try
                        {
                            // "Clear" Pin
                            FStreamOut[i].Position = 0;
                            FStreamOut[i].SetLength(0);

                            Texture2D.ToStream(this.AssignedContext.CurrentDeviceContext, this.FTextureIn[i][this.AssignedContext].Resource, this.FInFormat[i], FStreamOut[i]);

                            FStreamOut.Flush(true);

                            this.FOutValid[i] = true;
                        }
                        catch (Exception ex)
                        {
                            FLogger.Log(ex);
                            this.FOutValid[i] = false;
                        }
                    }
                    else
                    {
                        this.FOutValid[i] = false;
                    }
                }
            }
            else
            {
                this.FOutValid.SliceCount = 0;

            }
        }

        #endregion
    }
}
