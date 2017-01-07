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

namespace VVVV.DX11.Nodes.MSKinect
{
    [PluginInfo(Name = "RGB", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "flateric", 
	            Tags = "DX11, texture",
	            Help = "")]
    public class KinectColorTextureNode : KinectBaseTextureNode
    {
        private IntPtr depthread;
        private IntPtr depthwrite;

        private int width;
        private int height;

        [ImportingConstructor()]
        public KinectColorTextureNode(IPluginHost host)
        {
            this.InitBuffers();
        }

        private void InitBuffers()
        {
            this.width = 1920;
            this.height = 1080;

            this.depthread = Marshal.AllocHGlobal(1920*1080*4);
            this.depthwrite = Marshal.AllocHGlobal(1920 * 1080 * 4);
        }

        private void DepthFrameReady(object sender, ColorFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
                using (frame)
                {
                    lock (m_lock)
                    {
                        frame.CopyConvertedFrameDataToIntPtr(this.depthwrite,1920 * 1080 * 4, ColorImageFormat.Bgra);

                        IntPtr swap = this.depthread;
                        this.depthread = this.depthwrite;
                        this.depthwrite = swap;
                    }
                    this.FInvalidate = true;
                    this.frameindex = frame.RelativeTime.Ticks;
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
            get { return SlimDX.DXGI.Format.B8G8R8A8_UNorm; }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            lock (m_lock)
            {
                texture.WriteData(this.depthread, 1920 * 1080 * 4);
            }
        }

        protected override void OnRuntimeConnected()
        {
            this.runtime.ColorFrameReady += DepthFrameReady;
        }

        protected override void OnRuntimeDisconnected()
        {
            this.runtime.ColorFrameReady -= DepthFrameReady;
        }

        protected override void Disposing()
        {
            Marshal.FreeHGlobal(this.depthread);
            Marshal.FreeHGlobal(depthwrite);
        }

    }
}
