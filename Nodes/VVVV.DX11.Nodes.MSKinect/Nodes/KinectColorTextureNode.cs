using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using Microsoft.Kinect;

namespace VVVV.DX11.Nodes.MSKinect
{
    [PluginInfo(Name = "RGB", 
	            Category = "Kinect", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11, texture",
	            Help = "Returns an B8G8R8A8_UNorm formatted texture from the Kinects RGB camera")]
    public class KinectColorTextureNode : KinectBaseTextureNode
    {

        private byte[] colorimage;
        private ColorImageFormat oldformat = ColorImageFormat.Undefined;

        public KinectColorTextureNode()
        {
            this.colorimage = new byte[640 * 480 * 2];
        }

        private void ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            ColorImageFrame frame = e.OpenColorImageFrame();

            if (frame != null)
            {
                if (frame.Format != this.oldformat)
                {
                    int bpp = frame.Format == ColorImageFormat.RgbResolution640x480Fps30 ? 4 : 2;
                    this.colorimage = new byte[640 * 480 * bpp];

                    this.oldformat = frame.Format;
                    this.DisposeTextures();
                }

                this.FInvalidate = true;
                this.frameindex = frame.FrameNumber;

                lock (m_lock)
                {
                    frame.CopyPixelDataTo(this.colorimage);
                }

                frame.Dispose();
            }
        }

        protected override int Width
        {
            get { return 640; }
        }

        protected override int Height
        {
            get { return 480; }
        }

        protected override SlimDX.DXGI.Format Format
        {
            get 
            {
                return this.oldformat == ColorImageFormat.RgbResolution640x480Fps30 ? SlimDX.DXGI.Format.B8G8R8X8_UNorm
                    : SlimDX.DXGI.Format.R16_UNorm;
            }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            texture.WriteData<byte>(this.colorimage);
        }

        protected override void OnRuntimeConnected()
        {
            this.runtime.ColorFrameReady += ColorFrameReady;
        }

        protected override void OnRuntimeDisconnected()
        {
            this.runtime.ColorFrameReady -= ColorFrameReady;
        }
    }
}
