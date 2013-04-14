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
    [PluginInfo(Name = "World", Category = "Kinect", Version = "Microsoft", Author = "vux", Tags = "dx11,texture")]
    public unsafe class KinectWorldTextureNode : KinectBaseTextureNode
    {

        private float[] world0;
        private float[] world1;
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
                    this.world0 = new float[320 * 240 * 4];
                    this.world1 = new float[320 * 240 * 4];
                    this.skelpoints = new SkeletonPoint[320 * 240];
                    this.depthpixels = new DepthImagePixel[320 * 240];
                    this.width = 320;
                    this.height = 240;
                }
                else if (this.currentformat == DepthImageFormat.Resolution640x480Fps30)
                {
                    this.world0 = new float[640 * 480 * 4];
                    this.world1 = new float[640 * 480 * 4];
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
                this.FInvalidate = true;
                if (frame.FrameNumber != this.frameindex)
                {
                    this.RebuildBuffer(frame.Format, false);

                    this.frameindex = frame.FrameNumber;
                    frame.CopyDepthImagePixelDataTo(this.depthpixels);
                    int cnt = 0;
                    int img = 0;
                    //DepthImagePixel dp;
                    //dp.
                    this.runtime.Runtime.CoordinateMapper.MapDepthFrameToSkeletonFrame(frame.Format, this.depthpixels, this.skelpoints);
                    for (int h = 0; h < this.height; h++)
                    {
                        for (int w = 0; w < this.width; w++)
                        {
                            //this.runtime.Runtime.CoordinateMapper.
                            //SkeletonPoint sp = frame.MapToSkeletonPoint(w, h);
                            SkeletonPoint sp = this.skelpoints[img];
                            this.world0[cnt] = sp.X;
                            this.world0[cnt + 1] = sp.Y;
                            this.world0[cnt + 2] = sp.Z;
                            this.world0[cnt + 3] = 1.0f;
                            cnt += 4;
                            img++;
                        }
                    }

                    frame.Dispose();

                    lock (m_lock)
                    {
                        float[] tmp = this.world0;
                        this.world0 = this.world1;
                        this.world1 = tmp;
                    }
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
            fixed (float* f = &world1[0])
            {
                IntPtr ptr = new IntPtr(f);
                texture.WriteData(ptr, this.width * this.height * 4 * 4);
            }
           // texture.WriteData<float>(world1);
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
