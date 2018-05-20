using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.MSKinect.Lib;

using SlimDX.Direct3D9;
using SlimDX;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "Face", 
	            Category = "Kinect", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11",
	            Help = "Returns general face tracking data")]
    public class KinectFaceNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Output("Success")]
        protected ISpread<bool> FOutOK;

        [Output("Face Data")]
        protected ISpread<FaceTrackFrame> FOutFrame;

        [Output("Position", Order = 10)]
        protected ISpread<Vector3> FOutPosition;

        [Output("Rotation")]
        protected ISpread<Vector3> FOutRotation;

        [Output("Frame Index", IsSingle = true)]
        protected ISpread<int> FOutFrameIndex;


        private bool FInvalidateConnect = false;
        private bool FInvalidate = true;

        private KinectRuntime runtime;

        private byte[] colorImage;
        private short[] depthImage;
        private Skeleton[] skeletonData;
        private readonly Dictionary<int, SkeletonFaceTracker> trackedSkeletons = new Dictionary<int, SkeletonFaceTracker>();

        private bool first = true;
        private DepthImageFormat olddepth;

        [ImportingConstructor()]
        public KinectFaceNode(IPluginHost host)
        {
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.runtime.AllFrameReady -= KinectFaceNode_AllFrameReady;
                }

                if (this.FInRuntime.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        this.FInRuntime[0].AllFrameReady += KinectFaceNode_AllFrameReady;
                    }

                }

                this.FInvalidateConnect = false;
            }

            if (this.FInvalidate)
            {
                this.FOutOK.SliceCount = this.trackedSkeletons.Count;
                this.FOutPosition.SliceCount = this.trackedSkeletons.Count;
                this.FOutRotation.SliceCount = this.trackedSkeletons.Count;

                List<FaceTrackFrame> frames = new List<FaceTrackFrame>();

                int cnt = 0;
                foreach (int key in this.trackedSkeletons.Keys)
                {
                    SkeletonFaceTracker sft = this.trackedSkeletons[key];
                    if (sft.frame != null)
                    {
                        frames.Add((FaceTrackFrame)sft.frame.Clone());
                        this.FOutOK[cnt] = sft.frame.TrackSuccessful;
                        this.FOutPosition[cnt] = new Vector3(sft.frame.Translation.X, sft.frame.Translation.Y, sft.frame.Translation.Z);
                        this.FOutRotation[cnt] = new Vector3(sft.frame.Rotation.X, sft.frame.Rotation.Y, sft.frame.Rotation.Z) * (float)VMath.DegToCyc;

                        EnumIndexableCollection<FeaturePoint, PointF> pp = sft.frame.GetProjected3DShape();
                        EnumIndexableCollection<FeaturePoint, Vector3DF> p = sft.frame.Get3DShape();
                    }
                    else
                    {
                        this.FOutOK[cnt] = false;
                        this.FOutPosition[cnt] = Vector3.Zero;
                        this.FOutRotation[cnt] = Vector3.Zero;
                    }
                    cnt++;
                }

                this.FOutFrame.AssignFrom(frames);
            }
        }

        void KinectFaceNode_AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;
            SkeletonFrame skeletonFrame = null;

            colorImageFrame = e.OpenColorImageFrame();
            depthImageFrame = e.OpenDepthImageFrame();
            skeletonFrame = e.OpenSkeletonFrame();

            if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
            {
                if (colorImageFrame != null) { colorImageFrame.Dispose(); }
                if (depthImageFrame != null) { depthImageFrame.Dispose(); }
                if (skeletonFrame != null) { skeletonFrame.Dispose(); }
                return;
            }

            if (first)
            {
                first = false;
                this.olddepth = depthImageFrame.Format;
            }
            else
            {
                if (this.olddepth != depthImageFrame.Format)
                {
                    //Need a reset
                    if (this.depthImage != null) { this.depthImage = null; }

                    foreach (SkeletonFaceTracker sft in this.trackedSkeletons.Values)
                    {
                        sft.Dispose();
                    }

                    this.trackedSkeletons.Clear();
                    this.olddepth = depthImageFrame.Format;
                }
            }

            if (this.depthImage == null)
            {
                this.depthImage = new short[depthImageFrame.PixelDataLength];
            }

            if (this.colorImage == null)
            {
                this.colorImage = new byte[colorImageFrame.PixelDataLength];
            }

            if (this.skeletonData == null || this.skeletonData.Length != skeletonFrame.SkeletonArrayLength)
            {
                this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
            }

            colorImageFrame.CopyPixelDataTo(this.colorImage);
            depthImageFrame.CopyPixelDataTo(this.depthImage);
            skeletonFrame.CopySkeletonDataTo(this.skeletonData);

            foreach (Skeleton skeleton in this.skeletonData)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                    || skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                {
                    // We want keep a record of any skeleton, tracked or untracked.
                    if (!this.trackedSkeletons.ContainsKey(skeleton.TrackingId))
                    {
                        this.trackedSkeletons.Add(skeleton.TrackingId, new SkeletonFaceTracker());
                    }

                    // Give each tracker the upated frame.
                    SkeletonFaceTracker skeletonFaceTracker;
                    if (this.trackedSkeletons.TryGetValue(skeleton.TrackingId, out skeletonFaceTracker))
                    {
                        skeletonFaceTracker.OnFrameReady(this.runtime.Runtime, colorImageFrame.Format, colorImage, depthImageFrame.Format, depthImage, skeleton);
                        skeletonFaceTracker.LastTrackedFrame = skeletonFrame.FrameNumber;
                    }
                }
            }

            this.RemoveOldTrackers(skeletonFrame.FrameNumber);

            colorImageFrame.Dispose();
            depthImageFrame.Dispose();
            skeletonFrame.Dispose();

            this.FInvalidate = true;
        }

        public void ConnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        public void DisconnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        private void RemoveOldTrackers(int currentFrameNumber)
        {
            var trackersToRemove = new List<int>();

            foreach (var tracker in this.trackedSkeletons)
            {
                uint missedFrames = (uint)currentFrameNumber - (uint)tracker.Value.LastTrackedFrame;
                if (missedFrames > 50)
                {
                    // There have been too many frames since we last saw this skeleton
                    trackersToRemove.Add(tracker.Key);
                }
            }

            foreach (int trackingId in trackersToRemove)
            {
                this.RemoveTracker(trackingId);
            }
        }

        private void RemoveTracker(int trackingId)
        {
            this.trackedSkeletons[trackingId].Dispose();
            this.trackedSkeletons.Remove(trackingId);
        }

        private class SkeletonFaceTracker : IDisposable
        {
            private FaceTracker faceTracker;

            private SkeletonTrackingState skeletonTrackingState;

            public int LastTrackedFrame { get; set; }

            public FaceTrackFrame frame;

            public void Dispose()
            {
                if (this.faceTracker != null)
                {
                    this.faceTracker.Dispose();
                    this.faceTracker = null;
                }
            }

            public void DrawFaceModel()
            {
                if (this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    return;
                }
            }

            /// <summary>
            /// Updates the face tracking information for this skeleton
            /// </summary>
            internal void OnFrameReady(KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest)
            {
                this.skeletonTrackingState = skeletonOfInterest.TrackingState;

                if (this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    // nothing to do with an untracked skeleton.
                    return;
                }

                if (this.faceTracker == null)
                {
                    try
                    {
                        this.faceTracker = new FaceTracker(kinectSensor);
                    }
                    catch
                    {
                        this.faceTracker = null;
                    }
                }

                if (this.faceTracker != null)
                {
                    frame = this.faceTracker.Track(
                        colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest).Clone() as FaceTrackFrame;
                }
            }

            /*private struct FaceModelTriangle
            {
                public Point P1;
                public Point P2;
                public Point P3;
            }*/
        }

    }
}
