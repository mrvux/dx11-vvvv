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
using System.Runtime.InteropServices;

namespace VVVV.DX11.Nodes.MSKinect
{
    [PluginInfo(Name = "DepthRGB", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11, texture",
                Help = "Returns a G32R32F formatted texture whose pixels represent a UV map mapping pixels from depth to color space.")]
    public unsafe class KinectDepthColorTextureNode : KinectBaseTextureNode
    {
        private IntPtr depthData;
        private IntPtr colpoints;
 
        private int width;
        private int height;

        [ImportingConstructor()]
        public KinectDepthColorTextureNode(IPluginHost host)
        {
            this.InitBuffers();
        }

        private void InitBuffers()
        {
            this.width = 1920;
            this.height = 1080;

            this.depthData = Marshal.AllocHGlobal(512 * 424 * 2);

            this.colpoints = Marshal.AllocHGlobal(1920 * 1080 * 8);
        }

        private void DepthFrameReady(object sender, DepthFrameArrivedEventArgs e)
        {
            DepthFrame frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
                using (frame)
                {
                    lock (m_lock)
                    {
                        frame.CopyFrameDataToIntPtr(depthData, 512 * 424 * 2);
                        this.runtime.Runtime.CoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(depthData, 512 * 424 * 2, colpoints, 1920 * 1080 * 8);
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

            get { return SlimDX.DXGI.Format.R32G32_Float; }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            lock (m_lock)
            {
                texture.WriteData(this.colpoints, 1920 * 1080 * 8);
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

        protected override void Disposing()
        {
            Marshal.FreeHGlobal(this.colpoints);
            Marshal.FreeHGlobal(this.depthData);
        }

    }
}



                      