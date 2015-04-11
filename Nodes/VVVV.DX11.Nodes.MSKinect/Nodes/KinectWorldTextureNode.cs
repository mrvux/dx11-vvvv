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
    [PluginInfo(Name = "World", 
	            Category = "Kinect", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11, texture",
	            Help = "Returns a texture with world-space coordinates encoded in each pixel")]
    public unsafe class KinectWorldTextureNode : KinectBaseTextureNode
    {
        private DepthImagePixel[] depthpixels;
        private SkeletonPoint[] skelpoints;
        private DepthImageFormat currentformat = DepthImageFormat.Resolution320x240Fps30;
        private int width;
        private int height;

        public KinectWorldTextureNode()
        {
            this.RebuildBuffer(DepthImageFormat.Resolution320x240Fps30, true);
        }

        private void RebuildBuffer(DepthImageFormat format, bool force)
        {
            if (format != this.currentformat || force)
            {
                this.Resized = true;
                this.currentformat = format;
                if (this.currentformat == DepthImageFormat.Resolution320x240Fps30)
                {
                    this.skelpoints = new SkeletonPoint[320 * 240];
                    this.depthpixels = new DepthImagePixel[320 * 240];
                    this.width = 320;
                    this.height = 240;
                }
                else if (this.currentformat == DepthImageFormat.Resolution640x480Fps30)
                {
                    this.skelpoints = new SkeletonPoint[640 * 480];
                    this.depthpixels = new DepthImagePixel[640 * 480];
                    this.width = 640;
                    this.height = 480;
                }
            }

        }

        private void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            DepthImageFrame frame = e.OpenDepthImageFrame();

            if (frame != null)
            {
                if (frame.FrameNumber != this.frameindex)
                {
                    this.FInvalidate = true;
                    this.RebuildBuffer(frame.Format, false);

                    this.frameindex = frame.FrameNumber;
                    frame.CopyDepthImagePixelDataTo(this.depthpixels);

                    lock (m_lock)
                    {
                        this.runtime.Runtime.CoordinateMapper.MapDepthFrameToSkeletonFrame(frame.Format, this.depthpixels, this.skelpoints);
                    }
                    frame.Dispose();
                }
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
            get { return SlimDX.DXGI.Format.R32G32B32A32_Float; }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            lock (m_lock)
            {
                fixed (SkeletonPoint* f = &this.skelpoints[0])
                {
                    IntPtr ptr = new IntPtr(f);
                    texture.WriteData(ptr, this.width * this.height * 16);
                }
            }
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
