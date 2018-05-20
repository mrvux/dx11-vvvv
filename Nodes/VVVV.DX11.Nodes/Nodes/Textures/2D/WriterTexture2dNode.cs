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
    [PluginInfo(Name = "Writer", Category = "DX11.Texture", Version = "2d", Author = "vux", AutoEvaluate = true)]
    public class WriterTextureNode : IPluginEvaluate, IDX11ResourceDataRetriever
    {
        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureIn;

        [Input("Filename",StringType=StringType.Filename,DefaultString="render")]
        protected ISpread<string> FInPath;

        [Input("Format")]
        protected ISpread<ImageFileFormat> FInFormat;

        [Input("Create Folder", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<bool> FCreateFolder;

        [Input("Write", IsBang = true)]
        protected ISpread<bool> FInSave;

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


        #region IPluginEvaluate Members

        public void Evaluate(int SpreadMax)
        {
            this.FOutValid.SliceCount = 1;

            if (this.FTextureIn.IsConnected)
            {
                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null) { this.FOutValid.SliceCount = 0; return; }
                //Do NOT cache this, assignment done by the host

                for (int i = 0; i < SpreadMax; i++)
                {
                    if (this.FTextureIn[i].Contains(this.AssignedContext) && this.FInSave[i])
                    {
                        if (this.FCreateFolder[0])
                        {
                            string path = Path.GetDirectoryName(this.FInPath[i]);
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                        }

                        try
                        {
                            Texture2D.SaveTextureToFile(this.AssignedContext.CurrentDeviceContext, this.FTextureIn[i][this.AssignedContext].Resource, this.FInFormat[i], this.FInPath[i]);
                            this.FOutValid[0] = true;
                        }
                        catch (Exception ex)
                        {
                            FLogger.Log(ex);
                            this.FOutValid[0] = false;
                        }
                    }
                    else
                    {
                        this.FOutValid[0] = false;
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
