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
    [PluginInfo(Name = "RGBDepth", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "sebl", 
	            Tags = "DX11, texture",
                Help = "Returns a G32R32F formatted texture whose pixels represent a UV map mapping pixels from depth to color space. Enable Relative Lookup to use it as displacement texture.")]
    public unsafe class KinectColorDepthTextureNode : KinectBaseTextureNode
    {
        private object m_depthlock = new object();

        ushort[] depthData;

        private ColorSpacePoint[] colpoints;
        //private DepthSpacePoint[] colpoints;
        private float[] colorimage;

        //private SlimDX.DXGI.Format format;
        private int width;
        private int height;

        [Input("Relative Lookup", IsSingle = true, IsToggle = true, DefaultBoolean = false)]
        protected Pin<bool> FRelativeLookup;

        [ImportingConstructor()]
        public KinectColorDepthTextureNode(IPluginHost host)
        {
            this.InitBuffers();
        }

        private void InitBuffers()
        {
            //this.format = SlimDX.DXGI.Format.R16_UInt;
            this.width = 512;
            this.height = 424;

            this.depthData = new ushort[width * height];

            this.colpoints = new ColorSpacePoint[this.width * this.height];
            //this.colpoints = new DepthSpacePoint[this.width * this.height];
            this.colorimage = new float[this.width * this.height * 2];
        }

        private void DepthFrameReady(object sender, DepthFrameArrivedEventArgs e)
        {
            DepthFrame frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
                using (frame)
                {
                    lock (m_depthlock)
                    {
                        frame.CopyFrameDataToArray(depthData);

                        this.runtime.Runtime.CoordinateMapper.MapDepthFrameToColorSpace(depthData, colpoints);
                        //this.runtime.Runtime.CoordinateMapper.MapColorFrameToDepthSpace(depthData, colpoints);
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
                for (int i = 0; i < this.colpoints.Length; i++)
                {
                    if (FRelativeLookup[0])
                    {
                        this.colorimage[i * 2] = (float)VMath.Map(colpoints[i].X - i % this.width, 0, this.width, 0, 1, TMapMode.Float);
                        this.colorimage[i * 2 + 1] = (float)VMath.Map(colpoints[i].Y - VMath.Abs(i / this.width), 0, this.height, 0, 1, TMapMode.Float);
                    }
                    else
                    {
                        this.colorimage[i * 2] = (float)VMath.Map(colpoints[i].X, 0, this.width, 0, 1, TMapMode.Clamp);
                        this.colorimage[i * 2 + 1] = (float)VMath.Map(colpoints[i].Y, 0, this.height, 0, 1, TMapMode.Clamp);
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
            this.runtime.DepthFrameReady += DepthFrameReady;
        }

        protected override void OnRuntimeDisconnected()
        {
            this.runtime.DepthFrameReady -= DepthFrameReady;
        }

        protected override void Disposing()
        {
        }

    }
}



                      