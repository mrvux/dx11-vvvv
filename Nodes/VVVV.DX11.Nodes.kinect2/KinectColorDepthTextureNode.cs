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
        private IntPtr depthData;
        private IntPtr colpoints;
        private IntPtr convertedColPoints;

        private int width;
        private int height;

        [Input("Raw Data", IsSingle = true, IsToggle = true, DefaultBoolean = true)]
        protected Pin<bool> FRawData;

        [Input("Relative Lookup", IsSingle = true, IsToggle = true, DefaultBoolean = false)]
        protected Pin<bool> FRelativeLookup;

        [ImportingConstructor()]
        public KinectColorDepthTextureNode(IPluginHost host)
        {
            this.InitBuffers();
        }

        private void InitBuffers()
        {
            this.width = 512;
            this.height = 424;

            this.depthData = Marshal.AllocHGlobal(512 * 424 * 2);
            this.colpoints = Marshal.AllocHGlobal(512 * 424 * 8);
            this.convertedColPoints = Marshal.AllocHGlobal(512 * 424 * 8);
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
                        this.runtime.Runtime.CoordinateMapper.MapDepthFrameToColorSpaceUsingIntPtr(depthData, 512 * 424 * 2, colpoints, 512 * 424 * 8);

                        if (!this.FRawData[0])
                        {
                            float* col = (float*)this.colpoints;
                            float* conv = (float*)this.convertedColPoints;
                            if (FRelativeLookup[0])
                            {
                                for (int i = 0; i < 512 * 424;i++ )
                                {
                                    conv[i*2] = (float)VMath.Map(col[i*2] - i % 1920, 0, 1920, 0, 1, TMapMode.Float);
                                    conv[i*2+1] = (float)VMath.Map(col[i*2+1] - VMath.Abs(i / 1920), 0, 1080, 0, 1, TMapMode.Float);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < 512 * 424; i++)
                                {
                                    conv[i * 2] = (float)VMath.Map(col[i*2], 0, 1920, 0, 1, TMapMode.Clamp);
                                    conv[i * 2 + 1] = (float)VMath.Map(col[i*2+1], 0, 1080, 0, 1, TMapMode.Clamp);
                                }
                            }
                        }
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
                texture.WriteData(this.colpoints, this.width * this.height * 8);
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



                      