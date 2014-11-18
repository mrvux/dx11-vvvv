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
	            Help = "Returns a texture substracting user")]
    public class KinectBackGroundTextureNode : KinectBaseTextureNode
    {
        private BackgroundRemovedColorStream backgroundstream;
        private Skeleton[] skeletons; 
        private int currentlyTrackedSkeletonId;

        private byte[] bgdata;

        public KinectBackGroundTextureNode()
        {
            this.bgdata = new byte[640 * 480 * 4];
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
            lock (m_lock)
            {
                texture.WriteData<byte>(this.bgdata);
            }  
        }

        protected override void OnRuntimeConnected()
        {
            this.runtime.AllFrameReady += runtime_AllFrameReady;
            this.backgroundstream = new BackgroundRemovedColorStream(this.runtime.Runtime);
            this.backgroundstream.Enable(ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30);
            this.backgroundstream.BackgroundRemovedFrameReady += backgroundstream_BackgroundRemovedFrameReady;
        }

        void backgroundstream_BackgroundRemovedFrameReady(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {
            using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {
                    lock (m_lock)
                    {
                        backgroundRemovedFrame.CopyPixelDataTo(this.bgdata);
                    }
                    this.frameindex = (int)backgroundRemovedFrame.Timestamp;
                }
            }
        }

        private void runtime_AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var depthFrame = e.OpenDepthImageFrame())
            {
                if (null != depthFrame)
                {
                    this.backgroundstream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                }
            }

            using (var colorFrame = e.OpenColorImageFrame())
            {
                if (null != colorFrame)
                {
                    this.backgroundstream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
                }
            }

            using (var skeletonFrame = e.OpenSkeletonFrame())
            {
                if (null != skeletonFrame)
                {
                    if (this.skeletons == null)
                    {
                        this.skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletons);
                    this.backgroundstream.ProcessSkeleton(this.skeletons, skeletonFrame.Timestamp);
                }
            }

            this.ChooseSkeleton();
        }

        protected override void OnRuntimeDisconnected()
        {
            this.runtime.AllFrameReady -= runtime_AllFrameReady;
            if (this.backgroundstream != null)
            {
                this.backgroundstream.BackgroundRemovedFrameReady -= backgroundstream_BackgroundRemovedFrameReady;
                this.backgroundstream.Dispose();
            }
        }

        private void ChooseSkeleton()
        {
            var isTrackedSkeltonVisible = false;
            var nearestDistance = float.MaxValue;
            var nearestSkeleton = 0;

            foreach (var skel in this.skeletons)
            {
                if (null == skel)
                {
                    continue;
                }

                if (skel.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                if (skel.TrackingId == this.currentlyTrackedSkeletonId)
                {
                    isTrackedSkeltonVisible = true;
                    break;
                }

                if (skel.Position.Z < nearestDistance)
                {
                    nearestDistance = skel.Position.Z;
                    nearestSkeleton = skel.TrackingId;
                }
            }

            if (!isTrackedSkeltonVisible && nearestSkeleton != 0)
            {
                this.backgroundstream.SetTrackedPlayer(nearestSkeleton);
                this.currentlyTrackedSkeletonId = nearestSkeleton;
            }
        }
    }
}
