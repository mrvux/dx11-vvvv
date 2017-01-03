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

using Microsoft.Kinect.Toolkit.FaceTracking;
using FeralTic.DX11.Geometry;
using VVVV.MSKinect.Lib;

namespace VVVV.DX11.Nodes.MSKinect
{

    public static class Dummy
    {
        public static Vector3 SlimVector(this Vector3DF v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }

     [PluginInfo(Name = "Face", 
                Category = "DX11.Geometry", 
                Version = "Microsoft", 
                Author = "vux", 
                Tags = "DX11",
                Help = "Returns a geometry representing the tracked face")]
    public class KinectFaceMeshNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Face", CheckIfChanged = true)]
        protected Pin<FaceTrackFrame> FInFrame;

        [Output("Output", Order = 5)]
        protected Pin<DX11Resource<DX11IndexedGeometry>> FOutput;

        private bool FInvalidate = false;

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;
            if (this.FInFrame.PluginIO.IsConnected)
            {
                if (this.FInFrame.IsChanged)
                {
                    this.FOutput.SliceCount = this.FInFrame.SliceCount;
                    for (int i = 0; i < this.FInFrame.SliceCount; i++)
                    {
                        if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<DX11IndexedGeometry>(); }
                    }
                    this.FInvalidate = true;
                }
            }
            else
            {
                if (this.FOutput.SliceCount > 0)
                {
                    this.FOutput.SafeDisposeAll();
                    this.FOutput.SliceCount = 0;
                }
            }

        }



        public void Update(DX11RenderContext context)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                bool update = this.FInvalidate;
                DX11IndexedGeometry geom;
                if (!this.FOutput[i].Contains(context))
                {
                    geom = new DX11IndexedGeometry(context);
                    geom.InputLayout = Pos3Norm3Tex2Vertex.Layout;
                    geom.VertexSize = Pos3Norm3Tex2Vertex.VertexSize;
                    geom.HasBoundingBox = false;
                    geom.Topology = PrimitiveTopology.TriangleList;

                    var indexstream = new DataStream(KinectRuntime.FACE_INDICES.Length * 4, true, true);
                    indexstream.WriteRange(KinectRuntime.FACE_INDICES);
                    indexstream.Position = 0;

                    geom.IndexBuffer = new DX11IndexBuffer(context, indexstream, false, true);

                    geom.VerticesCount = this.FInFrame[i].GetProjected3DShape().Count;

                    var vbuffer = new SlimDX.Direct3D11.Buffer(context.Device, new BufferDescription()
                    {
                        BindFlags = BindFlags.VertexBuffer,
                        CpuAccessFlags = CpuAccessFlags.Write,
                        OptionFlags = ResourceOptionFlags.None,
                        SizeInBytes = geom.VerticesCount * geom.VertexSize,
                        Usage = ResourceUsage.Dynamic
                    });
                    geom.VertexBuffer = vbuffer;

                    this.FOutput[i][context] = geom;
                    update = true;
                }
                else
                {
                    geom = this.FOutput[i][context];
                }



                if (update)
                {
                    DataStream ds = geom.LockVertexBuffer();
                    ds.Position = 0;

                    EnumIndexableCollection<FeaturePoint, PointF> pp = this.FInFrame[i].GetProjected3DShape();
                    EnumIndexableCollection<FeaturePoint, Vector3DF> p = this.FInFrame[i].Get3DShape();

                    Vector3[] norms = new Vector3[p.Count];

                    int[] inds = KinectRuntime.FACE_INDICES;
                    int tricount = inds.Length / 3;
                    //Compute smoothed normals
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

                        norms[i1] += norm; norms[i2] += norm; norms[i3] += norm;

                    }


                    for (int j = 0; j < geom.VerticesCount; j++)
                    {
                        Pos3Norm3Tex2Vertex vertex = new Pos3Norm3Tex2Vertex();
                        Vector3DF v = p[j];
                        vertex.Position = new Vector3(v.X, v.Y, v.Z);
                        vertex.Normals = Vector3.Normalize(norms[j]);
                        vertex.TexCoords = new Vector2(0, 0);
                        ds.Write<Pos3Norm3Tex2Vertex>(vertex);
                    }


                    geom.UnlockVertexBuffer();
                }
            }
        }



        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutput.SafeDisposeAll(context);
        }

        public void Dispose()
        {
            this.FOutput.SafeDisposeAll();
        }
    }
}
