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
using Microsoft.Kinect.Face;
using VVVV.DX11;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "HDFaceBuffer",
                Category = "Kinect2",
                Version = "Microsoft",
                Author = "flateric",
                Tags = "DX11",
                Help = "Returns high definition face data for user")]
    [Obsolete("Does not support multiple devices")]
    public unsafe class KinectHDFaceBufferNode : IPluginEvaluate, IPluginConnections, IDX11ResourceHost, IDisposable
    {

        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Input("Rotation Check")]
        protected Pin<bool> FInRCheck;

        [Input("Tracking Id")]
        protected ISpread<string> FInId;

        [Input("Is Paused")]
        protected ISpread<bool> FInPaused;

        [Output("Face Vertices")]
        protected ISpread<DX11Resource<IDX11ReadableStructureBuffer>> FOutFaceVertices;

        [Output("Face UV")]
        protected ISpread<DX11Resource<IDX11ReadableStructureBuffer>> FOutFaceUV;

        [Output("Geometry")]
        protected ISpread<DX11Resource<DX11IndexOnlyGeometry>> FOutGeom;

        [Output("Rotation")]
        protected ISpread<Quaternion> FOutOrientation;

        [Output("BMin")]
        protected ISpread<Vector3> FOutBmin;

        [Output("BMax")]
        protected ISpread<Vector3> FOutBMax;

        [Output("Is Paused")]
        protected ISpread<bool> FOutPaused;

        [Output("TrackingId")]
        protected ISpread<ulong> FOuTrackingId;


        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;

        private HighDefinitionFaceFrameSource faceFrameSource = null;
        private HighDefinitionFaceFrameReader faceFrameReader = null;

        private FaceModel faceModel = new FaceModel();
        private FaceAlignment faceAlignment = new FaceAlignment();

        private bool FInvalidate = false;

        private Body[] lastframe = new Body[6];

        private object m_lock = new object();


        private DX11IndexBuffer ibo;
        private DX11DynamicStructuredBuffer<Vector3> faceVertexBuffer;
        private DX11DynamicStructuredBuffer<Vector2> faceUVBuffer;

        private CameraSpacePoint[] cameraPoints;
        private ColorSpacePoint[] colorPoints = new ColorSpacePoint[FaceModel.VertexCount];

        public KinectHDFaceBufferNode()
        {
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutGeom[0] == null)
            {
                this.FOutGeom[0] = new DX11Resource<DX11IndexOnlyGeometry>();
                this.FOutFaceVertices[0] = new DX11Resource<IDX11ReadableStructureBuffer>();
                this.FOutFaceUV[0] = new DX11Resource<IDX11ReadableStructureBuffer>();
            }

            if (this.FInvalidateConnect)
            {
                if (this.FInRuntime.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        //this.runtime.SkeletonFrameReady += SkeletonReady;
                        this.faceFrameSource = new HighDefinitionFaceFrameSource(this.runtime.Runtime);
                        this.faceFrameReader = this.faceFrameSource.OpenReader();
                        this.faceFrameReader.FrameArrived += this.faceReader_FrameArrived;
                        this.faceFrameReader.IsPaused = true;
                    }
                }
                else
                {
                    //this.runtime.SkeletonFrameReady -= SkeletonReady;
                    this.faceFrameReader.FrameArrived -= this.faceReader_FrameArrived;
                    this.faceFrameReader.Dispose();

                }

                this.FInvalidateConnect = false;
            }

            if (this.faceFrameSource != null)
            {
                ulong id = 0;
                try
                {
                    id = ulong.Parse(this.FInId[0]);
                }
                catch
                {

                }
                this.faceFrameSource.TrackingId = id;
                this.faceFrameReader.IsPaused = this.FInPaused[0];
            }

            

            this.FOutPaused[0] = this.faceFrameReader != null ? this.faceFrameReader.IsPaused : true;
        }


        void faceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (HighDefinitionFaceFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (frame.IsTrackingIdValid == false) { return; }
                    if (frame.FaceAlignmentQuality == FaceAlignmentQuality.Low) { return; }

                    frame.GetAndRefreshFaceAlignmentResult(this.faceAlignment);
                    var o = this.faceAlignment.FaceOrientation;
                    this.FOutOrientation[0] = new Quaternion(o.X, o.Y, o.Z, o.W);

                    if (this.FInRCheck[0])
                    {
                        float f = this.FOutOrientation[0].LengthSquared();
                        if (f > 0.1f)
                        {
                            this.cameraPoints = this.faceModel.CalculateVerticesForAlignment(this.faceAlignment).ToArray();
                            this.runtime.Runtime.CoordinateMapper.MapCameraPointsToColorSpace(this.cameraPoints, this.colorPoints);
                            SetBounds();
                            this.FInvalidate = true;
                        }
                    }
                    else
                    {
                        this.cameraPoints = this.faceModel.CalculateVerticesForAlignment(this.faceAlignment).ToArray();
                        this.runtime.Runtime.CoordinateMapper.MapCameraPointsToColorSpace(this.cameraPoints, this.colorPoints);
                        SetBounds();
                        this.FInvalidate = true;
                    }
                   

                }
            }
        }

        private void SetBounds()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            for (int i = 0; i < this.cameraPoints.Length ;i++)
            {
                CameraSpacePoint cp = this.cameraPoints[i];
                min.X = Math.Min(min.X, cp.X);
                min.Y = Math.Min(min.Y, cp.Y);
                min.Z = Math.Min(min.Z, cp.Z);

                max.X = Math.Max(max.X, cp.X);
                max.Y = Math.Max(max.Y, cp.Y);
                max.Z = Math.Max(max.Z, cp.Z);
            }

            this.FOutBmin[0] = min;
            this.FOutBMax[0] = max;

        }


        private void SkeletonReady(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame skeletonFrame = e.FrameReference.AcquireFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletonFrame.GetAndRefreshBodyData(this.lastframe);

                    bool found = false;
                    float minZ = float.MaxValue;
                    ulong cloestId = 0;

                    for (int i = 0; i < this.lastframe.Length; i++)
                    {
                        if (this.lastframe[i].IsTracked)
                        {
                            found = true;
                            var z = this.lastframe[i].Joints[JointType.Head].Position.Z;
                            if (z < minZ)
                            {
                                z = minZ;
                                cloestId = this.lastframe[i].TrackingId;
                            }
                        }
                    }

                    if (found)
                    {
                        this.faceFrameSource.TrackingId = cloestId;
                        this.FOuTrackingId[0] = this.faceFrameSource.TrackingId;                 
                        this.faceFrameReader.IsPaused = false;
                    }
                    else
                    {
                        this.faceFrameReader.IsPaused = true;
                        this.FOuTrackingId[0] =0;
                    }

                    

                    skeletonFrame.Dispose();
                }
            }
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

        public void Update(DX11RenderContext context)
        {
            if (this.ibo == null)
            {
                uint[] faceIndices = faceModel.TriangleIndices.ToArray();

                fixed (uint* uPtr = &faceIndices[0])
                {
                    DataStream ds = new DataStream(new IntPtr(uPtr), faceIndices.Length * 4, true, true);
                    this.ibo = new DX11IndexBuffer(context, ds, false, false);
                }

                this.FOutGeom[0][context] = new DX11IndexOnlyGeometry(context);
                this.FOutGeom[0][context].IndexBuffer = this.ibo;
            }

            if (this.faceVertexBuffer == null)
            {
                this.faceVertexBuffer = new DX11DynamicStructuredBuffer<Vector3>(context, (int)FaceModel.VertexCount);
                this.FOutFaceVertices[0][context] = this.faceVertexBuffer;

                this.faceUVBuffer = new DX11DynamicStructuredBuffer<Vector2>(context, (int)FaceModel.VertexCount);
                this.FOutFaceUV[0][context] = this.faceUVBuffer;
            }

            if (this.FInvalidate)
            {
                fixed (CameraSpacePoint* cPtr = &this.cameraPoints[0])
                {
                    this.faceVertexBuffer.WriteData(new IntPtr(cPtr));
                }
                fixed (ColorSpacePoint* cPtr = &this.colorPoints[0])
                {
                    this.faceUVBuffer.WriteData(new IntPtr(cPtr));
                }

                this.FInvalidate = false;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }

        public void Dispose()
        {
            if (this.FOutGeom[0] != null)
            {
                this.FOutGeom[0].Dispose();
            }

            if (this.FOutFaceVertices[0] != null)
            {
                this.FOutFaceVertices[0].Dispose();
            }
        }
    }
}
