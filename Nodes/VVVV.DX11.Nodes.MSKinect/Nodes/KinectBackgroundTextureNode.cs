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
using Microsoft.Kinect.Toolkit.BackgroundRemoval;

namespace VVVV.DX11.Nodes.MSKinect
{
    [PluginInfo(Name = "BackGround", 
	            Category = "Kinect", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11, texture",
	            Help = "returns backgrouns ")]
    public class KinectBackGroundTextureNode : KinectBaseTextureNode
    {
        [Input("Player ID")]
        protected ISpread<int> FInPlayerID;

        /*[Output("Average Depth")]
        protected ISpread<float> FOutAvg;*/

        private byte[] colorimage;
        private BackgroundRemovedColorStream bgstream;

        private Skeleton[] skeletons = new Skeleton[6];


        public KinectBackGroundTextureNode()
        {
            this.colorimage = new byte[640 * 480 * 4];
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
            get 
            {
                return SlimDX.DXGI.Format.B8G8R8A8_UNorm;
            }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            texture.WriteData<byte>(this.colorimage);
        }

        protected override void OnRuntimeConnected()
        {
            this.runtime.AllFrameReady += this.AllFrameReady;
            this.bgstream = new BackgroundRemovedColorStream(this.runtime.Runtime);
            this.bgstream.Enable(ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30);
            this.bgstream.BackgroundRemovedFrameReady += bgstream_BackgroundRemovedFrameReady;
        }

        void bgstream_BackgroundRemovedFrameReady(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {
            var frame = e.OpenBackgroundRemovedColorFrame();
            if (frame != null)
            {

                this.FInvalidate = true;
                /*this.FOutAvg[0] = frame.AverageDepth;*/

                lock (m_lock)
                {
                    frame.CopyPixelDataTo(this.colorimage);
                }

                frame.Dispose();

                /*if (null == this.foregroundBitmap || this.foregroundBitmap.PixelWidth != backgroundRemovedFrame.Width
                    || this.foregroundBitmap.PixelHeight != backgroundRemovedFrame.Height)
                {
                    this.foregroundBitmap = new WriteableBitmap(backgroundRemovedFrame.Width, backgroundRemovedFrame.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

                    // Set the image we display to point to the bitmap where we'll put the image data
                    this.MaskedColor.Source = this.foregroundBitmap;
                }

                // Write the pixel data into our bitmap
                this.foregroundBitmap.WritePixels(
                    new Int32Rect(0, 0, this.foregroundBitmap.PixelWidth, this.foregroundBitmap.PixelHeight),
                    backgroundRemovedFrame.GetRawPixelData(),
                    this.foregroundBitmap.PixelWidth * sizeof(int),
                    0);*/
            }
        }

        private void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            // in the middle of shutting down, or lingering events from previous sensor, do nothing here.
            /*if (null == this.runtime || null == this.sensorChooser.Kinect || this.sensorChooser.Kinect != sender)
            {
                return;
            }*/

            try
            {
                using (var depthFrame = e.OpenDepthImageFrame())
                {
                    if (null != depthFrame)
                    {
                        this.bgstream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                    }
                }

                using (var colorFrame = e.OpenColorImageFrame())
                {
                    if (null != colorFrame)
                    {
                        this.bgstream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
                    }
                }

                using (var skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (null != skeletonFrame)
                    {
                        skeletonFrame.CopySkeletonDataTo(this.skeletons);
                        this.bgstream.ProcessSkeleton(this.skeletons, skeletonFrame.Timestamp);
                    }
                }

                this.bgstream.SetTrackedPlayer(this.FInPlayerID[0]);

                /*this.ChooseSkeleton();*/
            }
            catch (InvalidOperationException)
            {
                // Ignore the exception. 
            }
        }

        protected override void OnRuntimeDisconnected()
        {
            this.bgstream.BackgroundRemovedFrameReady -= bgstream_BackgroundRemovedFrameReady;
            this.runtime.AllFrameReady -= AllFrameReady;
            this.bgstream.Disable();
        }

    }
}
