using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.DX11.Lib.Devices;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "FrameDelay", Category = "DX11.Texture", Version = "3d",
        AutoEvaluate=true,      
        Author = "vux",
        Warnings="non spreadable")]
    public class FrameDelayVolumeNode : IPluginEvaluate, IDX11ResourceHost, IDisposable, IDX11RenderStartPoint
    {
        [Input("Texture In", IsSingle = true, AutoValidate =false)]
        protected Pin<DX11Resource<DX11Texture3D>> FTextureInput;

        [Input("Enabled", IsSingle = true, DefaultValue = 1)]
        protected ISpread<bool> FInEnabled;

        [Input("Flush", IsSingle = true)]
        protected ISpread<bool> FInFlush;

        [Output("Texture Out", IsSingle = true, AllowFeedback=true)]
        protected Pin<DX11Resource<DX11Texture3D>> FTextureOutput;

        private DX11Texture3D lasttexture = null;
        private ILogger logger;

        public DX11RenderContext RenderContext
        {
            get { return DX11GlobalDevice.DeviceManager.RenderContexts[0]; }
        }

        public bool Enabled
        {
            get { return this.FInEnabled.SliceCount > 0 ? this.FInEnabled[0] : false; }
        }

        [ImportingConstructor()]
        public FrameDelayVolumeNode( ILogger logger)
        {
            this.logger = logger;
        }

        public void Present()
        {
            DX11RenderContext context = this.RenderContext;

            //Rendering is finished, so should be ok to grab texture now
            if (this.FTextureInput.IsConnected && this.FTextureInput.SliceCount > 0 && this.FTextureInput[0].Contains(context))
            {
                DX11Texture3D texture = this.FTextureInput[0][context];

                if (texture != null)
                {

                    if (this.lasttexture != null)
                    {
                        if (this.lasttexture.Resource.Description != texture.Resource.Description) { this.DisposeTexture(); }
                    }

                    if (this.lasttexture == null)
                    {
                        this.lasttexture = DX11Texture3D.FromDescription(context, texture.Resource.Description);
                    }

                    context.CurrentDeviceContext.CopyResource(texture.Resource, this.lasttexture.Resource);

                    if (this.FInFlush[0]) { context.CurrentDeviceContext.Flush(); }
                }

            }
            else
            {
                this.DisposeTexture();
            }
        }

        private void DisposeTexture()
        {
            if (this.lasttexture != null) { this.lasttexture.Dispose(); this.lasttexture = null; }
        }

        public void Evaluate(int SpreadMax)
        {
            if (SpreadMax > 0 && this.FInEnabled[0])
            {
                this.FTextureInput.Sync();
            }

            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11Texture3D>();
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.lasttexture != null)
            {
                this.FTextureOutput[0][context] = this.lasttexture;
            }
            else
            {
                this.FTextureOutput[0].Dispose(context);
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }

        public void Dispose()
        {
            this.DisposeTexture();
        }
    }
}
