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
using System.Runtime.InteropServices;

using Vector4 = SlimDX.Vector4;

namespace VVVV.DX11.Nodes.MSKinect
{
    [PluginInfo(Name = "World", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "flateric", 
	            Tags = "DX11, texture",
	            Help = "Returns world positions from the Kinects depth camera.")]
    public unsafe class KinectWorldTextureNode : KinectBaseTextureNode
    {
        private Vector4[] colorread;
        private Vector4[] colorwrite;

        private CameraSpacePoint[] camerawrite;
        private ushort[] depthwrite;

        private int width;
        private int height;

        [ImportingConstructor()]
        public KinectWorldTextureNode(IPluginHost host)
        {
            this.InitBuffers();
        }

        private void InitBuffers()
        {
            this.width = 512;
            this.height = 424;

            this.colorread = new Vector4[512 * 424];
            this.colorwrite = new Vector4[512 * 424];
            this.camerawrite = new CameraSpacePoint[512 * 424];
            this.depthwrite = new ushort[512 * 424];
        }
         
        private void DepthFrameReady(object sender, DepthFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
                using (frame)
                {
                    frame.CopyFrameDataToArray(this.depthwrite);
                    this.runtime.Runtime.CoordinateMapper.MapDepthFrameToCameraSpace(this.depthwrite, this.camerawrite);

                    lock (m_lock)
                    {
                        int pixels = 512*424;
                        for (int i = 0; i < pixels;i++)
                        {
                            this.colorwrite[i].X = this.camerawrite[i].X;
                            this.colorwrite[i].Y = this.camerawrite[i].Y;
                            this.colorwrite[i].Z = this.camerawrite[i].Z;
                        }

                        Vector4[] swap = this.colorread;
                        this.colorread = this.colorwrite;
                        this.colorwrite = swap;
                        this.frameindex = frame.RelativeTime.Ticks;
                    }

                    this.FInvalidate = true;
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
                fixed (Vector4* cp = &this.colorread[0])
                {
                    IntPtr ptr = new IntPtr(cp);
                    texture.WriteData(ptr, 512 * 424 * 16);
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
