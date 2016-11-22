using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect.Toolkit.FaceTracking;
using SlimDX;
using VVVV.DX11.Nodes.MSKinect;
using VVVV.MSKinect.Lib;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace MSKinect.Nodes
{
    [PluginInfo(Name = "FaceData", 
	            Category = "Kinect", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11",
	            Help = "Returns detailed 2D and 3D data describing the tracked face")]
    public class KinectFaceFrameDataNode : IPluginEvaluate
    {
        [Input("Face", CheckIfChanged = true)]
        protected Pin<FaceTrackFrame> FInFrame;

        [Output("Success")]
        protected ISpread<bool> FOutOK;

        [Output("Position")]
        protected ISpread<Vector3> FOutPosition;

        [Output("Rotation")]
        protected ISpread<Vector3> FOutRotation;

        [Output("Face Points")]
        protected ISpread<ISpread<Vector3>> FOutPts;

        [Output("Face Normals")]
        protected ISpread<ISpread<Vector3>> FOutNormals;
        
        [Output("Projected Face Points")]
        protected ISpread<ISpread<Vector2>> FOutPPTs;

        [Output("Indices")]
        protected ISpread<int> FOutIndices;

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
                        this.FOutRotation[cnt] = new Vector3(frame.Rotation.X, frame.Rotation.Y, frame.Rotation.Z) * (float)VMath.DegToCyc;

                        EnumIndexableCollection<FeaturePoint, PointF> pp = frame.GetProjected3DShape();
                        EnumIndexableCollection<FeaturePoint, Vector3DF> p = frame.Get3DShape();

                        this.FOutPPTs[cnt].SliceCount = pp.Count;
                        this.FOutPts[cnt].SliceCount = p.Count;
						this.FOutNormals[cnt].SliceCount = p.Count;

						//Compute smoothed normals
						Vector3[] norms = new Vector3[p.Count];
						int[] inds = KinectRuntime.FACE_INDICES;
						int tricount = inds.Length / 3;
						for (int j = 0; j < tricount; j++)
						{
							int i1 = inds[j * 3];
							int i2 = inds[j * 3 + 1];
							int i3 = inds[j * 3 + 2];

							Vector3 v1 = p[i1].SlimVector();
							Vector3 v2 = p[i2].SlimVector();
							Vector3 v3 = p[i3].SlimVector();

							Vector3 faceEdgeA = v2 - v1;
							Vector3 faceEdgeB = v1 - v3;
							Vector3 norm = Vector3.Cross(faceEdgeB, faceEdgeA);

							norms[i1] += norm; 
							norms[i2] += norm; 
							norms[i3] += norm;
						}
						
						for (int i = 0; i < pp.Count; i++)
						{
							this.FOutPPTs[cnt][i] = new Vector2(pp[i].X, pp[i].Y);
							this.FOutPts[cnt][i] = new Vector3(p[i].X, p[i].Y, p[i].Z);
							this.FOutNormals[cnt][i] = Vector3.Normalize(norms[i]);
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
