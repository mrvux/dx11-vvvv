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

using VVVV.Utils.VColor;
using Microsoft.Kinect;

namespace VVVV.DX11.Nodes.MSKinect
{
    [PluginInfo(Name = "BodyIndex", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11, texture",
	            Help = "")]
    public class KinectBodyIndexTextureNode : KinectBaseTextureNode
    {
        private byte[] rawdepth;

        public KinectBodyIndexTextureNode()
        {
            this.rawdepth = new byte[512 * 424];
        }

        protected override void OnEvaluate()
        {

        }

        protected override int Width
        {
            get { return 512; }
        }

        protected override int Height
        {
            get { return 424; }
        }

        protected override SlimDX.DXGI.Format Format
        {
            get { return SlimDX.DXGI.Format.R8_UInt; }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            lock (m_lock)
            {
                texture.WriteData<byte>(this.rawdepth);
            }      
        }

        protected override void OnRuntimeConnected()
        {
            this.runtime.BodyFrameReady += DepthFrameReady;
        }

        protected override void OnRuntimeDisconnected()
        {
            this.runtime.BodyFrameReady -= DepthFrameReady;
        }

        private void DepthFrameReady(object sender, BodyIndexFrameArrivedEventArgs e)
        {
            BodyIndexFrame frame = e.FrameReference.AcquireFrame();
            if (frame != null)
            {
                this.FInvalidate = true;
                this.frameindex = frame.RelativeTime.Ticks;
                lock (m_lock)
                {
                    frame.CopyFrameDataToArray(this.rawdepth);
                }
                frame.Dispose();
            }
        }
    }

}
