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
    [PluginInfo(Name = "Depth", 
	            Category = "Kinect", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11, texture",
	            Help = "Returns a 16bit depthmap from the Kinects depth camera.")]
    public class KinectDepthTextureNode : KinectBaseTextureNode
    {

        //private byte[] depthimage;
        private short[] rawdepth;

        private DepthImagePixel[] depthpixels;

        private int width;
        private int height;
        private bool first = true;
        private DepthImageFormat format;

        [ImportingConstructor()]
        public KinectDepthTextureNode(IPluginHost host)
        {

        }

        private void InitBuffers(DepthImageFrame frame)
        {
            this.format = frame.Format;
            this.width = frame.Width;
            this.height = frame.Height;
            this.depthpixels = new DepthImagePixel[frame.Width * frame.Height];
            this.rawdepth = new short[frame.Width * frame.Height];
        }

        private void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            DepthImageFrame frame = e.OpenDepthImageFrame();
            if (frame != null)
            {
                if (this.first || frame.Format != this.format)
                {
                    this.InitBuffers(frame);
                    this.DisposeTextures();
                }

                this.FInvalidate = true;
                this.frameindex = frame.FrameNumber;
                lock (m_lock)
                {
                    frame.CopyDepthImagePixelDataTo(this.depthpixels);
                    for (int i16 = 0; i16 < this.width * this.height; i16++)
                    {
                        this.rawdepth[i16] = this.depthpixels[i16].Depth;
                    }
                }

                frame.Dispose();
            }
        }



        protected override int Width
        {
            get { return this.width; }
        }

        protected override int Height
        {
            get { return this.height; }
        }

        protected override SlimDX.DXGI.Format Format
        {
            get { return SlimDX.DXGI.Format.R16_UNorm; }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            texture.WriteDataStride(this.rawdepth);
        }

        protected override void OnRuntimeConnected()
        {
            this.runtime.DepthFrameReady += DepthFrameReady;
        }

        protected override void OnRuntimeDisconnected()
        {
            this.runtime.DepthFrameReady -= DepthFrameReady;
        }
    }
}
