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
    [PluginInfo(Name = "Body",
                Category = "Kinect2",
                Version = "Split",
                Author = "flateric",
                Tags = "DX11",
                Help = "Splits bodies from body data structure")]
    public class KinectBodySplitNode : IPluginEvaluate
    {
        [Input("Bodies")]
        private Pin<Body> FInBodies;

        [Output("User Index")]
        private ISpread<int> FOutUserIndex;

        [Output("Position")]
        private ISpread<Vector3> FOutPosition;

        [Output("Clipping")]
        private ISpread<Vector4> FOutClipped;

        [Output("Joint ID")]
        private ISpread<string> FOutJointID;

        [Output("Joint Position")]
        private ISpread<Vector3> FOutJointPosition;

        [Output("Joint Orientation")]
        private ISpread<Quaternion> FOutJointOrientation;

        [Output("Joint State")]
        private ISpread<string> FOutJointState;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInBodies.PluginIO.IsConnected)
            {
                int cnt = this.FInBodies.SliceCount;
                this.FOutPosition.SliceCount = cnt;
                this.FOutUserIndex.SliceCount = cnt;
                this.FOutClipped.SliceCount = cnt;
                this.FOutJointPosition.SliceCount = cnt * 25;
                this.FOutJointState.SliceCount = cnt * 25;
                this.FOutJointID.SliceCount = cnt * 25;
                this.FOutJointOrientation.SliceCount = cnt * 25;

                                    int jc = 0;
                    for (int i = 0; i < cnt; i++)
                    {
                        Body sk = this.FInBodies[i];

                        Joint ce = sk.Joints[JointType.SpineBase];
                        this.FOutPosition[i] = new Vector3(ce.Position.X, ce.Position.Y, ce.Position.Z);
                        this.FOutUserIndex[i] = (int)sk.TrackingId;

                        Vector4 clip = Vector4.Zero;
                        clip.X = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Left));
                        clip.Y = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Right));
                        clip.Z = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Top));
                        clip.W = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Bottom));

                        this.FOutClipped[i] = clip;

                        foreach (Joint joint in sk.Joints.Values)
                        {
                            Microsoft.Kinect.Vector4 bo = sk.JointOrientations[joint.JointType].Orientation;
                            this.FOutJointID[jc] = joint.JointType.ToString();
                            this.FOutJointPosition[jc] = new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);

                            this.FOutJointOrientation[jc] = new Quaternion(bo.X, bo.Y, bo.Z, bo.W);
                            this.FOutJointState[jc] = joint.TrackingState.ToString();
                            jc++;
                        }
                    }
            }
            else
            {
                this.FOutPosition.SliceCount = 0;
                this.FOutUserIndex.SliceCount = 0;
                this.FOutJointID.SliceCount = 0;
                this.FOutJointPosition.SliceCount = 0;
                this.FOutJointState.SliceCount = 0;
                this.FOutJointOrientation.SliceCount = 0;
                this.FOutClipped.SliceCount = 0;
            }
        }
    }
}
