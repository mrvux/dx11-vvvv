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
    [PluginInfo(Name = "Player", 
	            Category = "Kinect", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11, texture",
	            Help = "")]
    public class KinectPlayeTextureNode : KinectBaseTextureNode
    {
        private int[] playerimage;
        private short[] rawdepth;

        private int[] colors = new int[9];

        [Input("Back Color", DefaultColor = new double[] { 0, 0, 0, 0 })]
        protected IDiffSpread<RGBAColor> FInBgColor;

        [Input("Player Color", DefaultColor = new double[] { 1, 0, 0, 0 })]
        protected IDiffSpread<RGBAColor> FInPlayerColor;

        private int width;
        private int height;
        private bool first = true;

        private void InitBuffers(DepthImageFrame frame)
        {
            this.width = frame.Width;
            this.height = frame.Height;
            this.playerimage = new int[width * height];
            this.rawdepth = new short[width * height];
            this.first = false;
            this.Resized = true;
        }

        public KinectPlayeTextureNode()
        {
            
        }

        protected override void OnEvaluate()
        {
            if (this.FInBgColor.IsChanged)
            {
                this.colors[0] = this.FInBgColor[0].Color.ToArgb();
                this.FInvalidate = true;
            }

            if (this.FInPlayerColor.IsChanged)
            {
                for (int i = 0; i < 8; i++)
                {
                    this.colors[i + 1] = this.FInPlayerColor[i].Color.ToArgb();
                }

                this.FInvalidate = true;
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
            texture.WriteData<int>(this.playerimage);
        }

        protected override void OnRuntimeConnected()
        {
            this.runtime.DepthFrameReady += DepthFrameReady;
        }

        protected override void OnRuntimeDisconnected()
        {
            this.runtime.DepthFrameReady -= DepthFrameReady;
        }


        private void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            DepthImageFrame frame = e.OpenDepthImageFrame();

            if (frame != null)
            {
                if (this.first || frame.Width != width || frame.Height != height)
                {
                    this.InitBuffers(frame);
                    this.DisposeTextures();
                    this.first = false;
                }

                this.FInvalidate = true;
                this.frameindex = frame.FrameNumber;
                lock (m_lock)
                {
                    frame.CopyPixelDataTo(this.rawdepth);
                    for (int i16 = 0; i16 < this.width * this.height; i16++)
                    {
                        int player = rawdepth[i16] & DepthImageFrame.PlayerIndexBitmask;
                        player = player % this.colors.Length;
                        this.playerimage[i16] = this.colors[player];

                    }
                }

                frame.Dispose();

            }
        }
    }

}
