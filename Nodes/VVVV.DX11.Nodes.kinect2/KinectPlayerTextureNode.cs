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
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11, texture",
	            Help = "")]
    public class KinectPlayeTextureNode : KinectBaseTextureNode
    {
        private int[] playerimage;
        private byte[] rawdepth;

        private int backcolor;
        private int[] colors = new int[9];

        [Input("Back Color", DefaultColor = new double[] { 0, 0, 0, 0 })]
        protected IDiffSpread<RGBAColor> FInBgColor;

        [Input("Player Color", DefaultColor = new double[] { 1, 0, 0, 0 })]
        protected IDiffSpread<RGBAColor> FInPlayerColor;

        public KinectPlayeTextureNode()
        {
            this.playerimage = new int[512 * 424];
            this.rawdepth = new byte[512 * 424];
        }

        protected override void OnEvaluate()
        {
            if (this.FInBgColor.IsChanged)
            {
                this.backcolor = this.FInBgColor[0].Color.ToArgb();
                this.FInvalidate = true;
            }

            if (this.FInPlayerColor.IsChanged)
            {
                for (int i = 0; i < 8; i++)
                {
                    this.colors[i] = this.FInPlayerColor[i].Color.ToArgb();
                }

                this.FInvalidate = true;
            }
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
            get { return SlimDX.DXGI.Format.B8G8R8A8_UNorm; }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            lock (m_lock)
            {
                texture.WriteData<int>(this.playerimage);
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
            int bg = this.backcolor;

            if (frame != null)
            {
                this.FInvalidate = true;
                this.frameindex = frame.RelativeTime.Ticks;
                lock (m_lock)
                {
                    frame.CopyFrameDataToArray(this.rawdepth);
                    for (int i16 = 0; i16 < 512 * 424; i16++)
                    {
                        byte player = rawdepth[i16];
                        this.playerimage[i16] = player == 255 ? bg : this.colors[player % 6];

                    }
                }

                frame.Dispose();
            }
        }
    }

}
