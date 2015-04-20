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
    [PluginInfo(Name = "RawPlayer", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "flateric", 
	            Tags = "DX11, texture",
	            Help = "Returns a 16bit depthmap from the Kinects depth camera. Raw texture, not samplable")]
    public class KinectPlayerTextureNodeRaw : KinectBaseTextureNode
    {
        private IntPtr bodyread;
        private IntPtr bodywrite;

        private SlimDX.DXGI.Format format;
        private int width;
        private int height;
        private bool first = true;

        [ImportingConstructor()]
        public KinectPlayerTextureNodeRaw(IPluginHost host)
        {
            this.InitBuffers();
        }

        private void InitBuffers()
        {
            this.format = SlimDX.DXGI.Format.R16_UInt;
            this.width = 512;
            this.height = 424;

            this.bodyread = Marshal.AllocHGlobal(512 * 424);
            this.bodywrite = Marshal.AllocHGlobal(512 * 424);
        }

        private void BodyFrameReady(object sender, BodyIndexFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
                using (frame)
                {
                    lock (m_lock)
                    {
                        frame.CopyFrameDataToIntPtr(this.bodywrite, 512 * 424);
                        IntPtr swap = this.bodyread;
                        this.bodyread = this.bodywrite;
                        this.bodywrite = swap;
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
            get { return SlimDX.DXGI.Format.R8_UInt; }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            lock (m_lock)
            {
                texture.WriteData(this.bodyread, 512 * 424);
            }
        }

        protected override void OnRuntimeConnected()
        {
            this.runtime.BodyFrameReady += BodyFrameReady;
        }

        protected override void OnRuntimeDisconnected()
        {
            this.runtime.BodyFrameReady -= BodyFrameReady;
        }

        protected override void Disposing()
        {
            Marshal.FreeHGlobal(this.bodyread);
            Marshal.FreeHGlobal(bodywrite);
        }
    }
}
