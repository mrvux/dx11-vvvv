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
    [PluginInfo(Name = "Skeleton", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "flateric", 
	            Tags = "DX11", 
	            Help = "Returns skeleton data for each tracked user")]
    public class KinectSkeletonNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Output("Skeleton Count", IsSingle = true)]
        protected ISpread<int> FOutCount;

        [Output("User Index")]
        protected ISpread<string> FOutUserIndex;

        [Output("Short Index")]
        protected ISpread<int> FOutShortIndex;

        [Output("Position")]
        protected ISpread<Vector3> FOutPosition;

        [Output("Clipping")]
        protected ISpread<Vector4> FOutClipped;

        [Output("Joint ID")]
        protected ISpread<string> FOutJointID;

        [Output("Joint Position")]
        protected ISpread<Vector3> FOutJointPosition;

        [Output("Joint Position RGB")]
        protected ISpread<Vector2> FOutJointPositionRGB;

        [Output("Joint Orientation")]
        protected ISpread<Quaternion> FOutJointOrientation;

        [Output("Joint State")]
        protected ISpread<string> FOutJointState;

        [Output("Frame Number", IsSingle = true)]
        protected ISpread<long> FOutFrameNumber;


        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;

        private bool FInvalidate = false;

        private Body[] lastframe = new Body[6];
        private object m_lock = new object();
        private long frameid = -1;

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
                    List<int> indices = new List<int>();
                    lock (m_lock)
                    {

                        for (int i = 0; i < this.lastframe.Length; i++)
                        {
                            if (this.lastframe[i].IsTracked)
                            {
                                skels.Add(this.lastframe[i]);
                                indices.Add(i);
                            }
                        }
                    }

                    int cnt = skels.Count;
                    this.FOutCount[0] = cnt;

                    this.FOutPosition.SliceCount = cnt;
                    this.FOutUserIndex.SliceCount = cnt;
                    this.FOutShortIndex.SliceCount = cnt;
                    this.FOutClipped.SliceCount = cnt;
                    this.FOutJointPosition.SliceCount = cnt * 25;
                    this.FOutJointState.SliceCount = cnt * 25;
                    this.FOutJointID.SliceCount = cnt * 25;
                    this.FOutJointPositionRGB.SliceCount = cnt * 25;
                    this.FOutJointOrientation.SliceCount = cnt * 25;
                    this.FOutFrameNumber[0] = this.frameid;


                    int jc = 0;
                    for (int i = 0; i < cnt; i++)
                    {
                        Body sk = skels[i];

                        Joint ce = sk.Joints[JointType.SpineBase];
                        this.FOutPosition[i] = new Vector3(ce.Position.X, ce.Position.Y, ce.Position.Z);
                        this.FOutUserIndex[i] = sk.TrackingId.ToString();
                        this.FOutShortIndex[i] = indices[i];

                        Vector4 clip = Vector4.Zero;
                        clip.X = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Left));
                        clip.Y = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Right));
                        clip.Z = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Top));
                        clip.W = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Bottom));

                        this.FOutClipped[i] = clip;

                        foreach (Joint joint in sk.Joints.Values)
                        {

                            var jrgb = this.runtime.Runtime.CoordinateMapper.MapCameraPointToColorSpace(joint.Position);

                            Microsoft.Kinect.Vector4 bo = sk.JointOrientations[joint.JointType].Orientation;
                            this.FOutJointID[jc] = joint.JointType.ToString();
                            this.FOutJointPosition[jc] = new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);

                            this.FOutJointOrientation[jc] = new Quaternion(bo.X, bo.Y, bo.Z, bo.W);
                            this.FOutJointState[jc] = joint.TrackingState.ToString();

                            this.FOutJointPositionRGB[jc] = new Vector2(jrgb.X, jrgb.Y);
                            jc++;
                        }
                    }
                }
                else
                {
                    this.FOutCount[0] = 0;
                    this.FOutPosition.SliceCount = 0;
                    this.FOutUserIndex.SliceCount = 0;
                    this.FOutJointID.SliceCount = 0;
                    this.FOutJointPosition.SliceCount = 0;
                    this.FOutJointState.SliceCount = 0;
                    this.FOutFrameNumber[0] = 0;
                    this.FOutJointOrientation.SliceCount = 0;
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
