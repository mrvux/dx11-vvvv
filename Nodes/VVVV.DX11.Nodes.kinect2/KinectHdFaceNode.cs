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

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "HDFace", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "flateric", 
	            Tags = "DX11", 
	            Help = "Returns high definition face data for each tracked user")]
    public class KinectHDFaceNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        private Pin<KinectRuntime> FInRuntime;

        /*[Output("Position Infrared")]
        private ISpread<Vector2> FOutPositionInfrared;

        [Output("Size Infrared")]
        private ISpread<Vector2> FOutSizeInfrared;

        [Output("Position Color")]
        private ISpread<Vector2> FOutPositionColor;

        [Output("Size Color")]
        private ISpread<Vector2> FOutSizeColor;

        [Output("Orientation")]
        private ISpread<Quaternion> FOutOrientation;

        [Output("Mouth Open")]
        private ISpread<DetectionResult> FOutMouthOpen;*/

        [Output("Vertices")]
        private ISpread<Vector3> FOutVertices;

        [Output("Indices")]
        private ISpread<uint> FOutIndices;


        [Output("Frame Number", IsSingle = true)]
        private ISpread<long> FOutFrameNumber;

        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;

        private HighDefinitionFaceFrameSource[] faceFrameSources = null;
        private HighDefinitionFaceFrameReader[] faceFrameReaders = null;

        private FaceModel faceModel = new FaceModel();
        private FaceAlignment faceAlignment = new FaceAlignment();
//private HighDefinitionFaceFrameResult[] lastResults = null;

        private bool FInvalidate = false;

        private Body[] lastframe = new Body[6];

        private object m_lock = new object();
        private int frameid = -1;

        private FaceModelBuilder faceModelBuilder = null;

        public KinectHDFaceNode()
        {
            faceFrameReaders = new HighDefinitionFaceFrameReader[6];
            faceFrameSources = new HighDefinitionFaceFrameSource[6];
        }

        private int GetIndex(HighDefinitionFaceFrameSource src)
        {
            for (int i = 0; i < faceFrameSources.Length;i++)
            {
                if (src == faceFrameSources[i]) { return i; }
            }
            return 0;
        }

        private bool first = true;

        public void Evaluate(int SpreadMax)
        {
            if (first)
            {
                var vertices = this.faceModel.CalculateVerticesForAlignment(this.faceAlignment);
                this.FOutVertices.SliceCount = vertices.Count;

                this.FOutIndices.SliceCount = this.faceModel.TriangleIndices.Count;
                this.FOutIndices.AssignFrom(this.faceModel.TriangleIndices);
                this.first = false;
            }

            if (this.FInvalidateConnect)
            {
                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    this.runtime.SkeletonFrameReady += SkeletonReady;

                    if (runtime != null)
                    {
                        for (int i = 0; i < this.faceFrameSources.Length; i++)
                        {
                            this.faceFrameSources[i] = new HighDefinitionFaceFrameSource(this.runtime.Runtime);
                            this.faceFrameReaders[i] = this.faceFrameSources[i].OpenReader();
                            this.faceFrameReaders[i].FrameArrived += this.faceReader_FrameArrived;

                            
                        }

                        this.faceModelBuilder = this.faceFrameSources[0].OpenModelBuilder(FaceModelBuilderAttributes.None);

                        this.faceModelBuilder.BeginFaceDataCollection();

                        this.faceModelBuilder.CollectionCompleted += this.HdFaceBuilder_CollectionCompleted;
                    }
                }
                else
                {
                    this.runtime.SkeletonFrameReady -= SkeletonReady;
                    for (int i = 0; i < this.faceFrameSources.Length; i++)
                    {
                        this.faceFrameReaders[i].FrameArrived -= this.faceReader_FrameArrived;
                        this.faceFrameReaders[i].Dispose();
                        //this.faceFrameSources[i].Dispose();
                    }
                }

                this.FInvalidateConnect = false;
            }

            this.FOutVertices.Flush(true);
        }

        private void HdFaceBuilder_CollectionCompleted(object sender, FaceModelBuilderCollectionCompletedEventArgs e)
        {
            var modelData = e.ModelData;

            this.faceModel = modelData.ProduceFaceModel();

            this.faceModelBuilder.Dispose();
            this.faceModelBuilder = null;
        }

        void faceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (HighDefinitionFaceFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (frame.IsTrackingIdValid == false) { return; }
                    frame.GetAndRefreshFaceAlignmentResult(this.faceAlignment);
                    var vertices = this.faceModel.CalculateVerticesForAlignment(this.faceAlignment);

                    for (int i = 0; i < vertices.Count; i++)
                    {
                        var v = vertices[i];
                        this.FOutVertices[i] = new Vector3(v.X, v.Y, v.Z);
                    }

                    this.FOutFrameNumber[0] = frame.RelativeTime.Ticks;
                    //var res = frame.FaceModel.CalculateVerticesForAlignment(FaceAlignmen;
                    /*if(res != null)
                    {
                        this.FOutFrameNumber[0] = (int)frame.FaceFrameResult.RelativeTime.Ticks;

                        Vector2 pos;
                        Vector2 size;

                        size.X = res.FaceBoundingBoxInColorSpace.Right - res.FaceBoundingBoxInColorSpace.Left;
                        //size.X /= 1920.0f;

                        size.Y = res.FaceBoundingBoxInColorSpace.Bottom - res.FaceBoundingBoxInColorSpace.Top;
                        //size.Y /= 1080.0f;

                        pos.X = size.X / 2.0f + (float)res.FaceBoundingBoxInColorSpace.Left;
                        pos.Y = size.Y / 2.0f + (float)res.FaceBoundingBoxInColorSpace.Top;

                        this.FOutPositionColor[0] = pos;
                        this.FOutSizeColor[0] = size;
                        
                        this.FOutOrientation[0] = new Quaternion(res.FaceRotationQuaternion.X, res.FaceRotationQuaternion.Y, 
                            res.FaceRotationQuaternion.Z, res.FaceRotationQuaternion.W);

                        this.FOutMouthOpen[0] = res.FaceProperties[FaceProperty.MouthOpen];
                    } */
                }
            }
        }


        private void SkeletonReady(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame skeletonFrame = e.FrameReference.AcquireFrame())
            {
                if (skeletonFrame != null)
                {

                   skeletonFrame.GetAndRefreshBodyData(this.lastframe);

                   for (int i = 0; i < this.lastframe.Length; i++)
                    {
                        if (this.faceFrameSources[0].IsTrackingIdValid)
                        {

                        }
                        else
                        {
                            if (this.lastframe[i].IsTracked)
                            {
                                this.faceFrameSources[0].TrackingId = this.lastframe[i].TrackingId;
                            }
                        }
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
