using System;
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
    [PluginInfo(Name = "Info", Category = "DX11.Texture", Version = "1d", Author = "vux", AutoEvaluate=false)]
    public class InfoTexture1dNode : IPluginEvaluate, IDX11ResourceDataRetriever
    {
        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture1D>> FTextureIn;

        [Output("Width")]
        protected ISpread<int> FOutWidth;

        [Output("Array Size")]
        protected ISpread<int> FOutArraySize;

        [Output("Format")]
        protected ISpread<SlimDX.DXGI.Format> FOutFormat;

        [Output("Mip Levels")]
        protected ISpread<int> FOutMipLevels;

        [Output("Resource Pointer", Visibility=PinVisibility.OnlyInspector)]
        protected ISpread<int> FOutPointer;

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
            if (this.FTextureIn.PluginIO.IsConnected)
            {
                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null) { this.SetNull(); return; }
                //Do NOT cache this, assignment done by the host

                this.FOutWidth.SliceCount = this.FTextureIn.SliceCount;
                this.FOutMipLevels.SliceCount = this.FTextureIn.SliceCount;
                this.FOutFormat.SliceCount = this.FTextureIn.SliceCount;
                this.FOutArraySize.SliceCount = this.FTextureIn.SliceCount;
                this.FOutPointer.SliceCount = this.FTextureIn.SliceCount;

                for (int i = 0; i < this.FTextureIn.SliceCount; i++)
                {
                    try
                    {
                        if (this.FTextureIn[i].Contains(this.AssignedContext))
                        {
                            Texture1DDescription tdesc = this.FTextureIn[i][this.AssignedContext].Resource.Description;
                            this.FOutWidth[i] = tdesc.Width;
                            this.FOutFormat[i] = tdesc.Format;
                            this.FOutMipLevels[i] = tdesc.MipLevels;
                            this.FOutArraySize[i] = tdesc.ArraySize;
                            this.FOutPointer[i] = this.FTextureIn[i][this.AssignedContext].Resource.ComPointer.ToInt32();
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
            this.FOutWidth.SliceCount = 0;
            this.FOutFormat.SliceCount = 0;
            this.FOutMipLevels.SliceCount = 0;
            this.FOutArraySize.SliceCount = 0;
            this.FOutPointer.SliceCount = 0;
        }

        private void SetDefault(int i)
        {
            this.FOutWidth[i] = -1;
            this.FOutFormat[i] = SlimDX.DXGI.Format.Unknown;
            this.FOutMipLevels[i] = -1;
            this.FOutArraySize[i] = -1;
            this.FOutPointer[i] = -1;
        }




    }
}
