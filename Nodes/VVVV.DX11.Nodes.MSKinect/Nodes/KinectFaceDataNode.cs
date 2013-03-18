using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX;
using Microsoft.Kinect.Toolkit.FaceTracking;
using VVVV.MSKinect.Lib;

namespace MSKinect.Nodes
{
    [PluginInfo(Name = "FaceData", Category = "Kinect", Version = "Microsoft", Author = "vux", Tags = "")]
    public class KinectFaceFrameDataNode : IPluginEvaluate
    {

        [Input("Face", CheckIfChanged = true)]
        private Pin<FaceTrackFrame> FInFrame;

        [Output("Success")]
        private ISpread<bool> FOutOK;

        [Output("Position")]
        private ISpread<Vector3> FOutPosition;

        [Output("Rotation")]
        private ISpread<Vector3> FOutRotation;

        [Output("Face Points")]
        private ISpread<ISpread<Vector3>> FOutPts;

        [Output("Projected Face Points")]
        private ISpread<ISpread<Vector2>> FOutPPTs;

        [Output("Indices")]
        private ISpread<int> FOutIndices;

        private float INVTWOPI = 0.5f / (float)Math.PI;

        private bool first = true;

        public void Evaluate(int SpreadMax)
        {
            //Output static indices all the time
            if (this.first)
            {
                this.FOutIndices.AssignFrom(KinectRuntime.FACE_INDICES);
                this.first = false;
            }

            if (this.FInFrame.PluginIO.IsConnected)
            {
                if (this.FInFrame.IsChanged)
                {
                    this.FOutOK.SliceCount = FInFrame.SliceCount;
                    this.FOutPosition.SliceCount = FInFrame.SliceCount;
                    this.FOutRotation.SliceCount = FInFrame.SliceCount;
                    this.FOutPts.SliceCount = FInFrame.SliceCount;
                    this.FOutPPTs.SliceCount = FInFrame.SliceCount;

                    for (int cnt = 0; cnt < this.FInFrame.SliceCount; cnt++)
                    {
                        FaceTrackFrame frame = this.FInFrame[cnt];
                        this.FOutOK[cnt] = frame.TrackSuccessful;
                        this.FOutPosition[cnt] = new Vector3(frame.Translation.X, frame.Translation.Y, frame.Translation.Z);
                        this.FOutRotation[cnt] = new Vector3(frame.Rotation.X, frame.Rotation.Y, frame.Rotation.Z) * INVTWOPI;

                        EnumIndexableCollection<FeaturePoint, PointF> pp = frame.GetProjected3DShape();
                        EnumIndexableCollection<FeaturePoint, Vector3DF> p = frame.Get3DShape();

                        this.FOutPPTs[cnt].SliceCount = pp.Count;
                        this.FOutPts[cnt].SliceCount = p.Count;

                        for (int i = 0; i < pp.Count; i++)
                        {
                            this.FOutPPTs[cnt][i] = new Vector2(pp[i].X, pp[i].Y);
                            this.FOutPts[cnt][i] = new Vector3(p[i].X, p[i].Y, p[i].Z);
                        }


                        /*FaceTriangle[] d = frame.GetTriangles();
                        this.FOutIndices.SliceCount = d.Length * 3;
                        for (int i = 0; i < d.Length; i++)
                        {
                            this.FOutIndices[i * 3] = d[i].First;
                            this.FOutIndices[i * 3 + 1] = d[i].Second;
                            this.FOutIndices[i * 3 + 2] = d[i].Third;
                        }*/
                    }
                }
            }
            else
            {
                this.FOutPosition.SliceCount = 0;
                this.FOutPPTs.SliceCount = 0;
                this.FOutPts.SliceCount = 0;
                this.FOutRotation.SliceCount = 0;
                this.FOutOK.SliceCount = 0;
            }
        }
    }
}
