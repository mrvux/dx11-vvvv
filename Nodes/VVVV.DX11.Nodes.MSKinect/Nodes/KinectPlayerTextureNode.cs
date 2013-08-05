﻿using System;
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
        private IDiffSpread<RGBAColor> FInBgColor;

        [Input("Player Color", DefaultColor = new double[] { 1, 0, 0, 0 })]
        private IDiffSpread<RGBAColor> FInPlayerColor;

        public KinectPlayeTextureNode()
        {
            this.playerimage = new int[640 * 480];
            this.rawdepth = new short[640 * 480];
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
            get { return 640; }
        }

        protected override int Height
        {
            get { return 480; }
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
                this.FInvalidate = true;
                this.frameindex = frame.FrameNumber;
                lock (m_lock)
                {
                    frame.CopyPixelDataTo(this.rawdepth);
                    for (int i16 = 0; i16 < 640 * 480; i16++)
                    {
                        int player = rawdepth[i16] & DepthImageFrame.PlayerIndexBitmask;
                        this.playerimage[i16] = this.colors[player];

                    }
                }

                frame.Dispose();
            }
        }
    }

}
