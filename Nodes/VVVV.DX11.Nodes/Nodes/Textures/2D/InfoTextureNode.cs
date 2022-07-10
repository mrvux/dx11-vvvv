﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Info", Category = "DX11.Texture", Version = "2d", Author = "vux", AutoEvaluate=false)]
    public class InfoTexture2dNode : IPluginEvaluate, IDX11ResourceDataRetriever
    {
        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureIn;

        [Output("Width")]
        protected ISpread<int> FOutWidth;

        [Output("Height")]
        protected ISpread<int> FOutHeight;

        [Output("Array Size")]
        protected ISpread<int> FOutArraySize;

        [Output("Format")]
        protected ISpread<SlimDX.DXGI.Format> FOutFormat;

        [Output("View Format")]
        protected ISpread<SlimDX.DXGI.Format> FOutViewFormat;

        [Output("Samples Count")]
        protected ISpread<int> FOutSampleCount;

        [Output("AA Quality")]
        protected ISpread<int> FOutAAQuality;

        [Output("Mip Levels")]
        protected ISpread<int> FOutMipLevels;

        [Output("Resource Pointer", Visibility=PinVisibility.OnlyInspector)]
        protected ISpread<long> FOutPointer;

        [Output("Creation Time", Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FOutCreationTime;

        [Import()]
        protected IPluginHost FHost;

        public DX11RenderContext AssignedContext
        {
            get;
            set; 
        }

        public event DX11RenderRequestDelegate RenderRequest;


        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            if (this.FTextureIn.IsConnected)
            {
                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null) { this.SetNull(); return; }
                //Do NOT cache this, assignment done by the host

                this.FOutWidth.SliceCount = this.FTextureIn.SliceCount;
                this.FOutHeight.SliceCount = this.FTextureIn.SliceCount;
                this.FOutMipLevels.SliceCount = this.FTextureIn.SliceCount;
                this.FOutFormat.SliceCount = this.FTextureIn.SliceCount;
                this.FOutViewFormat.SliceCount = this.FTextureIn.SliceCount;

                this.FOutSampleCount.SliceCount = this.FTextureIn.SliceCount;
                this.FOutAAQuality.SliceCount = this.FTextureIn.SliceCount;
                this.FOutArraySize.SliceCount = this.FTextureIn.SliceCount;
                this.FOutPointer.SliceCount = this.FTextureIn.SliceCount;
                this.FOutCreationTime.SliceCount = this.FTextureIn.SliceCount;
                
                for (int i = 0; i < this.FTextureIn.SliceCount; i++)
                {
                    try
                    {
                        if (this.FTextureIn[i].Contains(this.AssignedContext))
                        {
                            if (this.FTextureIn[i][this.AssignedContext] != null)
                            {
                                var tex = this.FTextureIn[i][this.AssignedContext];
                                Texture2DDescription tdesc = tex.Resource.Description;
                                this.FOutHeight[i] = tdesc.Height;
                                this.FOutWidth[i] = tdesc.Width;
                                this.FOutFormat[i] = tdesc.Format;
                                this.FOutViewFormat[i] = tex.SRV.Description.Format;
                                this.FOutMipLevels[i] = tdesc.MipLevels;
                                this.FOutSampleCount[i] = tdesc.SampleDescription.Count;
                                this.FOutAAQuality[i] = tdesc.SampleDescription.Quality;
                                this.FOutArraySize[i] = tdesc.ArraySize;
                                this.FOutPointer[i] = this.FTextureIn[i][this.AssignedContext].Resource.ComPointer.ToInt64();
                                this.FOutCreationTime[i] = this.FTextureIn[i][this.AssignedContext].Resource.CreationTime;
                            }
                            else
                            {
                                this.SetDefault(i);
                            }
                        }
                        else
                        {
                            this.SetDefault(i);
                        }
                    }
                    catch
                    {
                        this.SetDefault(i);
                    }
                }
            }
            else
            {
                this.SetNull();
            }
        }

        #endregion

        private void SetNull()
        {
            this.FOutHeight.SliceCount = 0;
            this.FOutWidth.SliceCount = 0;
            this.FOutFormat.SliceCount = 0;
            this.FOutViewFormat.SliceCount = 0;
            this.FOutMipLevels.SliceCount = 0;
            this.FOutSampleCount.SliceCount = 0;
            this.FOutAAQuality.SliceCount = 0;
            this.FOutArraySize.SliceCount = 0;
            this.FOutPointer.SliceCount = 0;
            this.FOutCreationTime.SliceCount = 0;

        }

        private void SetDefault(int i)
        {
            this.FOutHeight[i] = -1;
            this.FOutWidth[i] = -1;
            this.FOutFormat[i] = SlimDX.DXGI.Format.Unknown;
            this.FOutViewFormat[i] = SlimDX.DXGI.Format.Unknown;
            this.FOutMipLevels[i] = -1;
            this.FOutSampleCount[i] = -1;
            this.FOutAAQuality[i] = -1;
            this.FOutArraySize[i] = -1;
            this.FOutPointer[i] = -1;
            this.FOutCreationTime[i] = 0;
        }




    }
}
