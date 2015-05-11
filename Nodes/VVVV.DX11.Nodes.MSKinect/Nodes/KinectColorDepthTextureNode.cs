using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils;
using VVVV.Utils.VMath;
using FeralTic.DX11;
using FeralTic.DX11.Resources;

using Microsoft.Kinect;

namespace VVVV.DX11.Nodes.MSKinect
{

    [PluginInfo(Name = "RGBDepth",
                Category = "Kinect",
                Version = "Microsoft",
                Author = "tmp",
                Tags = "DX11, texture",
                Help = "Returns a G32R32F formatted texture whose pixels represent a UV map mapping pixels from depth to color space. Enable Relative Lookup to use it as displacement texture.")]
    public unsafe class KinectColorDepthTextureNode : KinectBaseTextureNode
    {
        private DepthImagePixel[] depthpixels;
        private ColorImagePoint[] colpoints;
        private float[] colorimage;
        private DepthImageFormat currentformat = DepthImageFormat.Resolution320x240Fps30;
        private int width;
        private int height;

        [Input("Relative Lookup", IsSingle = true, IsToggle = true, DefaultBoolean = false)]
        protected Pin<bool> FRelativeLookup;

        public KinectColorDepthTextureNode()
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
                    this.colpoints = new ColorImagePoint[320 * 240];
                    this.colorimage = new float[320 * 240 * 2];
                    this.depthpixels = new DepthImagePixel[320 * 240];
                    this.width = 320;
                    this.height = 240;
                }
                else if (this.currentformat == DepthImageFormat.Resolution640x480Fps30)
                {
                    this.colpoints = new ColorImagePoint[640 * 480];
                    this.colorimage = new float[640 * 480 * 2];
                    this.depthpixels = new DepthImagePixel[640 * 480];
                    this.width = 640;
                    this.height = 480;
                }
            }

        }

        private void AllFrameReady(object sender, AllFramesReadyEventArgs e)
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
                        this.runtime.Runtime.CoordinateMapper.MapDepthFrameToColorFrame(frame.Format, this.depthpixels, ColorImageFormat.RgbResolution640x480Fps30, this.colpoints);
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
            get { return SlimDX.DXGI.Format.R32G32_Float; }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {

            lock (m_lock)
            {
                for (int i = 0; i < this.colpoints.Length; i++)
                {
                    if (FRelativeLookup[0])
                    {
                        int stepX = (640 / this.width);
                        int stepY = (480 / this.height);
                        this.colorimage[i * 2] = (float)VMath.Map(colpoints[i].X - (i * stepX) % 640, 0, 640, 0, 1, TMapMode.Float);
                        this.colorimage[i * 2 + 1] = (float)VMath.Map(colpoints[i].Y - (i * stepX * stepY / 640), 0, 480, 0, 1, TMapMode.Float);
                    }
                    else
                    {
                        this.colorimage[i * 2] = (float)VMath.Map(colpoints[i].X, 0, 640, 0, 1, TMapMode.Clamp);
                        this.colorimage[i * 2 + 1] = (float)VMath.Map(colpoints[i].Y, 0, 480, 0, 1, TMapMode.Clamp);
                    }
                }

                fixed (float* f = &this.colorimage[0])
                {
                    IntPtr ptr = new IntPtr(f);
                    texture.WriteData(ptr, this.width * this.height * 8);
                }
            }
        }

        protected override void OnRuntimeConnected()
        {
            this.runtime.AllFrameReady += AllFrameReady;
        }

        protected override void OnRuntimeDisconnected()
        {
            this.runtime.AllFrameReady -= AllFrameReady;
        }
    }
}