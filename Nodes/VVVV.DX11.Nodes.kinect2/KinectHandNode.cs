using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX;
using VVVV.MSKinect.Lib;
using VVVV.PluginInterfaces.V1;
using Microsoft.Kinect;
using Vector4 = SlimDX.Vector4;
using Quaternion = SlimDX.Quaternion;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "Hand", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "flateric", 
	            Tags = "DX11", 
	            Help = "Returns hand data from a tracked user, including if hand is opened or closed")]
    public class KinectHandNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Output("Skeleton Count", IsSingle = true)]
        protected ISpread<int> FOutCount;

        [Output("User Index")]
        protected ISpread<int> FOutUserIndex;

        [Output("Left Position")]
        protected ISpread<Vector3> FOutLPosition;

        [Output("Left State")]
        protected ISpread<HandState> FOutLState;

        [Output("Left Confidence")]
        protected ISpread<TrackingConfidence> FOutLConfidence;

        [Output("Right Position")]
        protected ISpread<Vector3> FOutRPosition;

        [Output("Right Confidence")]
        protected ISpread<TrackingConfidence> FOutRConfidence;

        [Output("Right State")]
        protected ISpread<HandState> FOutRState;

        [Output("Frame Number", IsSingle = true)]
        protected ISpread<int> FOutFrameNumber;


        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;

        private bool FInvalidate = false;

        private Body[] lastframe = new Body[6];
        private object m_lock = new object();
        private int frameid = -1;

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
                if (this.lastframe != null)
                {
                    List<Body> skels = new List<Body>();
                    lock (m_lock)
                    {

                        foreach (Body sk in this.lastframe)
                        {
                            if (sk.IsTracked)
                            {
                                skels.Add(sk);
                            }
                        }
                    }

                    int cnt = skels.Count;
                    this.FOutCount[0] = cnt;

                    this.FOutLPosition.SliceCount = cnt;
                    this.FOutRPosition.SliceCount = cnt;
                    FOutLConfidence.SliceCount = cnt;
                    FOutRConfidence.SliceCount = cnt;
                    FOutLState.SliceCount = cnt;
                    FOutRState.SliceCount = cnt;
                    this.FOutUserIndex.SliceCount = cnt;
                    this.FOutFrameNumber[0] = this.frameid;



                    for (int i = 0; i < cnt; i++)
                    {
                        Body sk = skels[i];

                        Joint lhand = sk.Joints[JointType.HandLeft];
                        this.FOutLPosition[i] = new Vector3(lhand.Position.X, lhand.Position.Y, lhand.Position.Z);

                        Joint rhand = sk.Joints[JointType.HandRight];
                        this.FOutRPosition[i] = new Vector3(rhand.Position.X, rhand.Position.Y, rhand.Position.Z);

                        FOutLConfidence[i] = sk.HandLeftConfidence;
                        FOutLState[i] = sk.HandLeftState;

                        FOutRConfidence[i] = sk.HandRightConfidence;
                        FOutRState[i] = sk.HandRightState;
                        
                        
                        this.FOutUserIndex[i] = (int)sk.TrackingId;


                    }
                }
                else
                {
                    this.FOutCount[0] = 0;
                    this.FOutLPosition.SliceCount = 0;
                    this.FOutRPosition.SliceCount = 0;
                    FOutRState.SliceCount = 0;
                    FOutRConfidence.SliceCount = 0;
                    FOutLState.SliceCount = 0;
                    FOutLConfidence.SliceCount = 0;
                    this.FOutUserIndex.SliceCount = 0;
                    this.FOutFrameNumber[0] = 0;
                }
                this.FInvalidate = false;
            }
        }

        private void SkeletonReady(object sender, BodyFrameArrivedEventArgs e)
        {


            using (BodyFrame skeletonFrame = e.FrameReference.AcquireFrame())
            {
                if (skeletonFrame != null)
                {
                    this.frameid = (int)e.FrameReference.RelativeTime.Ticks;
                    lock (m_lock)
                    {
                        skeletonFrame.GetAndRefreshBodyData(this.lastframe);
                    }
                    skeletonFrame.Dispose();
                }
            }
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
    }
}
