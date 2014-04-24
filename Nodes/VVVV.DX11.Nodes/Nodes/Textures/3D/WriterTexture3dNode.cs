﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using SlimDX.Direct3D11;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Writer", Category = "DX11.Texture", Version = "3d", Author = "vux", AutoEvaluate = true)]
    public class WriterTexture3dNode : IPluginEvaluate, IDX11ResourceDataRetriever
    {
        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture3D>> FTextureIn;

        [Input("Filename",StringType=StringType.Filename,DefaultString="render")]
        protected ISpread<string> FInPath;

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

            if (this.FTextureIn.PluginIO.IsConnected)
            {
                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null) { this.FOutValid.SliceCount = 0; return; }
                //Do NOT cache this, assignment done by the host
                DX11RenderContext context = this.AssignedContext;

                for (int i = 0; i < SpreadMax; i++)
                {
                    if (this.FTextureIn[i].Contains(context) && this.FInSave[i])
                    {
                        try
                        {
                            string path = this.FInPath[i];
                            if (!path.EndsWith(".dds", StringComparison.InvariantCultureIgnoreCase))
                            {
                                path += ".dds";
                            }
                            Texture3D.SaveTextureToFile(this.AssignedContext.CurrentDeviceContext, this.FTextureIn[i][context].Resource, ImageFileFormat.Dds, path);
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
