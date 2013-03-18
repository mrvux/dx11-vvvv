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
    [PluginInfo(Name = "Depth", Category = "Kinect", Version = "Microsoft", Author = "vux", Tags = "dx11,texture")]
    public class KinectDepthTextureNode : KinectBaseTextureNode
    {

        //private byte[] depthimage;
        private short[] rawdepth;

        private DepthImagePixel[] depthpixels;

        [ImportingConstructor()]
        public KinectDepthTextureNode(IPluginHost host)
        {
            this.depthpixels = new DepthImagePixel[320 * 240];
            this.rawdepth = new short[320 * 240];
        }

        private void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            DepthImageFrame frame = e.OpenDepthImageFrame();

            if (frame != null)
            {
                this.FInvalidate = true;
                this.frameindex = frame.FrameNumber;
                lock (m_lock)
                {
                    frame.CopyDepthImagePixelDataTo(this.depthpixels);
                    for (int i16 = 0; i16 < 320 * 240; i16++)
                    {
                        this.rawdepth[i16] = this.depthpixels[i16].Depth;
                    }
                }

                frame.Dispose();
            }
        }



        protected override int Width
        {
            get { return 320; }
        }

        protected override int Height
        {
            get { return 240; }
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
