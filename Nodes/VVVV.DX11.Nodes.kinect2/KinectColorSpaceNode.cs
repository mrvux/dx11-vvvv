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
    [PluginInfo(Name = "ColorSpace", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "flateric", 
	            Tags = "DX11, texture",
	            Help = "Returns a 16bit depthmap from the Kinects depth camera.")]
    public class KinectColorSpaceTextureNode : KinectBaseTextureNode
    {
        private object m_depthlock = new object();
        private ushort[] depthread;
        private ushort[] depthwrite;

        private ColorSpacePoint[] points;

        private SlimDX.DXGI.Format format;
        private int width;
        private int height;
        private bool first = true;

        [ImportingConstructor()]
        public KinectColorSpaceTextureNode(IPluginHost host)
        {
            this.InitBuffers();
        }

        private void InitBuffers()
        {
            this.format = SlimDX.DXGI.Format.R32G32_Float;
            this.width = 512;
            this.height = 424;

            this.depthread = new ushort[512 * 424];
            this.depthwrite = new ushort[512 * 424];
            this.points = new ColorSpacePoint[512 * 424];
        }

        private void DepthFrameReady(object sender, DepthFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
                using (frame)
                {
                    lock (m_depthlock)
                    {
                        frame.CopyFrameDataToArray(this.depthwrite);

                        this.runtime.Runtime.CoordinateMapper.MapDepthFrameToColorSpace(this.depthwrite, this.points);

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
            get { return SlimDX.DXGI.Format.R16_UNorm; }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            texture.WriteData<ColorSpacePoint>(this.points);
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
