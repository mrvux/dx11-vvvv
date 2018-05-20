using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using SlimDX.Direct3D11;
using System.ComponentModel.Composition;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes.Textures
{
    [PluginInfo(Name = "AsSharedTexture", Category = "DX11.Texture", Version = "2d", Author = "vux,tonfilm", AutoEvaluate =true)]
    public class ToSharedTextureNode : IPluginEvaluate, IDX11ResourceDataRetriever, IDisposable
    {
        [Import()]
        protected IPluginHost FHost;

        [Input("Texture In", IsSingle=true)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureIn;

        [Output("Pointer",IsSingle=true, AsInt=true)]
        protected ISpread<long> FPointer;

        private Texture2D tex = null;
        private SlimDX.DXGI.Resource SharedResource = null;

        public void Evaluate(int SpreadMax)
        {
            if (this.FTextureIn.IsConnected)
            {

                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null) { this.SetNull(); return; }

                this.FPointer.SliceCount = SpreadMax;

                DX11RenderContext context = this.AssignedContext;


                try
                {
                    if (this.FTextureIn[0].Contains(context))
                    {
                        if (tex != null)
                        {
                            Texture2D t = this.FTextureIn[0][context].Resource;

                            if (t.Description.Width != this.tex.Description.Width 
                                || t.Description.Height != this.tex.Description.Height
                                || t.Description.Format != this.tex.Description.Format)
                            {
                                this.SharedResource.Dispose();
                                this.SharedResource = null;
                                this.tex.Dispose();
                                this.tex = null;
                            }


                            if (t.Description.MipLevels > 1)
                            {
                                this.FPointer[0] = 0;
                                throw new Exception("Sharing texture with more than one mip level is not allowed");
                            }
                            if (t.Description.SampleDescription.Count > 1)
                            {
                                this.FPointer[0] = 0;
                                throw new Exception("Sharing multisampled texture is not allowed");
                            }

                        }
                        //Convert texture so it has no mips
                        if (tex == null)
                        {

                            Texture2D t = this.FTextureIn[0][context].Resource;
                            Texture2DDescription desc = t.Description;
                            desc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
                            desc.OptionFlags = ResourceOptionFlags.Shared;
                            desc.MipLevels = 1;
                            this.tex = new Texture2D(context.Device, desc);
                            this.SharedResource = new SlimDX.DXGI.Resource(this.tex);
                            this.FPointer[0] = SharedResource.SharedHandle.ToInt64();
                        }

                        this.AssignedContext.CurrentDeviceContext.CopyResource(this.FTextureIn[0][context].Resource, this.tex);
                    }
                    else
                    {
                        this.SetDefault(0);
                    }
                }
                catch
                {
                    this.SetDefault(0);
                }
            }
            else
            {
                this.SetNull();
            }
        }


        private void SetNull()
        {
            this.FPointer.SliceCount = 0;
        }

        private void SetDefault(int i)
        {
            this.FPointer[i] = 0;
        }

        public void Dispose()
        {
            try { tex?.Dispose(); }
            catch { }
        }

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;
    }
}
