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
    [PluginInfo(Name = "FrameDelay", Category = "DX11.Texture", Version = "2d",
        AutoEvaluate=true,      
        Author = "vux",
        Warnings="Doesn't suppport multicontext, experimental,non spreadable")]
    public class FrameDelayTextureNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Texture In", IsSingle = true)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureInput;

        [Input("Flush", IsSingle = true)]
        protected ISpread<bool> FInFlush;

        [Output("Texture Out", IsSingle = true, AllowFeedback=true)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureOutput;

        private IHDEHost hde;
        private DX11Texture2D lasttexture = null;
        private ILogger logger;

        [ImportingConstructor()]
        public FrameDelayTextureNode(IHDEHost hde, ILogger logger)
        {
            this.hde = hde;
            this.hde.MainLoop.OnResetCache += this.MainLoop_OnPresent;
            this.logger = logger;
        }

        private void MainLoop_OnPresent(object sender, EventArgs e)
        {
            //Rendering is finished, so should be ok to grab texture now
            if (this.FTextureInput.PluginIO.IsConnected && this.FTextureInput.SliceCount > 0)
            {

                //Little temp hack, grab context from global, since for now we have one context anyway
                DX11RenderContext context = DX11GlobalDevice.DeviceManager.RenderContexts[0];

                if (this.FTextureInput[0].Contains(context))
                {
                    DX11Texture2D texture = this.FTextureInput[0][context];

                    if (texture is DX11DepthStencil)
                    {
                        this.logger.Log(LogType.Warning, "FrameDelay for depth texture is not supported");
                        return;
                    }

                    if (this.lasttexture != null)
                    {
                        if (this.lasttexture.Description != texture.Description) { this.DisposeTexture(); }
                    }

                    if (this.lasttexture == null)
                    {
                        this.lasttexture = DX11Texture2D.FromDescription(context, texture.Description);
                    }

                    context.CurrentDeviceContext.CopyResource(texture.Resource, this.lasttexture.Resource);

                    if (this.FInFlush[0]) { context.CurrentDeviceContext.Flush(); }
                }
                else
                {
                    this.DisposeTexture();
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
            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11Texture2D>();
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
            this.hde.MainLoop.OnResetCache -= this.MainLoop_OnPresent;
        }
    }
}
