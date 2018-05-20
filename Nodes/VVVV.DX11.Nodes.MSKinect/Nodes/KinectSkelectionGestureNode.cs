using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.MSKinect.Lib;
using VVVV.PluginInterfaces.V1;
using Microsoft.Kinect;
using Fizbin.Kinect.Gestures;

namespace VVVV.DX11.MSKinect.Nodes
{
    

    [PluginInfo(Name = "Gesture", 
	            Category = "Kinect", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Credits="https://github.com/EvilClosetMonkey/Fizbin.Kinect.Gestures", 
	            Tags = "DX11",
	            Help = "Returns tracked gesture data")]
    public class KinectSkelectionGestureNode : IPluginEvaluate, IPluginConnections
    {
        private class GestureFrame
        {
            public GestureType Gesture;
            public bool IsNew;
        }


        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Output("Skeleton Id")]
        protected ISpread<int> FOutId;

        [Output("Gesture Type")]
        protected ISpread<GestureType> FOutType;

        [Output("Is New")]
        protected ISpread<bool> FOutNew;

        [Output("Skeleton Count", IsSingle = true)]
        protected ISpread<int> FOutCount;

        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;

        private bool FInvalidate = true;

        private Skeleton[] lastframe = null;
        private object m_lock = new object();

        private GestureController gestureController;

        private Dictionary<int, GestureFrame> LastGestures = new Dictionary<int, GestureFrame>();

        public KinectSkelectionGestureNode()
        {
            gestureController = new GestureController();
            gestureController.GestureRecognized += OnGestureRecognized;
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.runtime.SkeletonFrameReady -= SkeletonReady;
                }

                if (this.FInRuntime.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        this.FInRuntime[0].SkeletonFrameReady += SkeletonReady;
                    }

                }

                this.FInvalidateConnect = false;
            }

            if (this.FInvalidate)
            {
                lock (m_lock)
                {
                    this.FOutId.SliceCount = this.LastGestures.Count;
                    this.FOutType.SliceCount = this.LastGestures.Count;

                    int cnt = 0;
                    foreach (int k in this.LastGestures.Keys)
                    {
                        this.FOutId[cnt] = k;

                         GestureFrame gf = this.LastGestures[k];

                        this.FOutType[cnt] = gf.Gesture;


                        this.FOutNew[cnt] = gf.IsNew;

                        gf.IsNew = false;
                        cnt++;
                    }
                }
                
                this.FInvalidate = false;
            }
        }

        private void SkeletonReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    this.lastframe = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(this.lastframe);

                    List<int> trackedids = new List<int>();

                    foreach (var skeleton in lastframe)
                    {
                        // skip the skeleton if it is not being tracked
                        if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                            continue;

                        trackedids.Add(skeleton.TrackingId);

                        // update the gesture controller
                        gestureController.UpdateAllGestures(skeleton);
                    }

                    lock (m_lock)
                    {
                        List<int> toremove = new List<int>();

                        foreach (int k in this.LastGestures.Keys)
                        {
                            if (!trackedids.Contains(k)) { toremove.Add(k); }
                        }

                        foreach (int k in toremove) { LastGestures.Remove(k); }
                    }
                    

                    skeletonFrame.Dispose();
                }
            }
            this.FInvalidate = true;
        }

        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            lock (m_lock)
            {
                GestureFrame gf = new GestureFrame();
                gf.Gesture = e.GestureType;
                gf.IsNew = true;
                this.LastGestures[e.TrackingId] = gf;
            }

            /*switch (e.GestureType)
            {
                case GestureType.Menu:
                    Gesture = "Menu";
                    break;
                case GestureType.WaveRight:
                    Gesture = "Wave Right";
                    break;
                case GestureType.WaveLeft:
                    Gesture = "Wave Left";
                    break;
                case GestureType.JoinedHands:
                    Gesture = "Joined Hands";
                    break;
                case GestureType.SwipeLeft:
                    Gesture = "Swipe Left";
                    break;
                case GestureType.SwipeRight:
                    Gesture = "Swipe Right";
                    break;
                case GestureType.ZoomIn:
                    Gesture = "Zoom In";
                    break;
                case GestureType.ZoomOut:
                    Gesture = "Zoom Out";
                    break;

                default:
                    break;
            }*/
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
    }
}
