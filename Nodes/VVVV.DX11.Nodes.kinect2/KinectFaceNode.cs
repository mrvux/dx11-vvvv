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
    [PluginInfo(Name = "Face", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "flateric", 
	            Tags = "DX11", 
	            Help = "Returns face data for each tracked user")]
    public class KinectFaceNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Output("Position Infrared")]
        protected ISpread<Vector2> FOutPositionInfrared;

        [Output("Size Infrared")]
        protected ISpread<Vector2> FOutSizeInfrared;

        [Output("Position Color")]
        protected ISpread<Vector2> FOutPositionColor;

        [Output("Size Color")]
        protected ISpread<Vector2> FOutSizeColor;

        [Output("Points Color")]
        protected ISpread<ISpread<Vector2>> FOutPointsColor;

        [Output("Points World")]
        protected ISpread<ISpread<Vector3>> FOutPointsWorld;

        [Output("Orientation")]
        protected ISpread<Quaternion> FOutOrientation;

        [Output("Engaged")]
        protected ISpread<string> FOutEngaged;

        [Output("Wear Glasses")]
        protected ISpread<string> FOutWearGlasses;

        [Output("Happy")]
        protected ISpread<string> FOutHappy;

        [Output("Left Eye Closed")]
        protected ISpread<string> FOutLeftEyeClosed;

        [Output("Right Eye Closed")]
        protected ISpread<string> FOutRightEyeClosed;

        [Output("Looking Away")]
        protected ISpread<string> FOutlookAway;

        [Output("Mouth Open")]
        protected ISpread<string> FOutMouthOpen;

        [Output("Mouth Moved")]
        protected ISpread<string> FOutMouthMoved;

        [Output("User Index")]
        protected ISpread<int> FOutUserIndex;

        [Output("Frame Number", IsSingle = true)]
        protected ISpread<int> FOutFrameNumber;

        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;

        private FaceFrameSource[] faceFrameSources = null;
        private FaceFrameReader[] faceFrameReaders = null;
        private FaceFrameResult[] lastResults = null;

        private Body[] lastframe = new Body[6];

        private object m_lock = new object();

        public KinectFaceNode()
        {
            faceFrameReaders = new FaceFrameReader[6];
            faceFrameSources = new FaceFrameSource[6];
            lastResults = new FaceFrameResult[6];
        }

        private int GetIndex(FaceFrameSource src)
        {
            for (int i = 0; i < faceFrameSources.Length;i++)
            {
                if (src == faceFrameSources[i]) { return i; }
            }
            return 0;
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (this.FInRuntime.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    this.runtime.SkeletonFrameReady += SkeletonReady;

                    if (runtime != null)
                    {
                        FaceFrameFeatures faceFrameFeatures =
                            FaceFrameFeatures.BoundingBoxInColorSpace
                            | FaceFrameFeatures.PointsInColorSpace
                            | FaceFrameFeatures.RotationOrientation
                            | FaceFrameFeatures.FaceEngagement
                            | FaceFrameFeatures.Glasses
                            | FaceFrameFeatures.Happy
                            | FaceFrameFeatures.LeftEyeClosed
                            | FaceFrameFeatures.RightEyeClosed
                            | FaceFrameFeatures.LookingAway
                            | FaceFrameFeatures.MouthMoved
                            | FaceFrameFeatures.MouthOpen;

                        for (int i = 0; i < this.faceFrameSources.Length; i++)
                        {
                            this.faceFrameSources[i] = new FaceFrameSource(this.runtime.Runtime, 0, faceFrameFeatures);
                            this.faceFrameReaders[i] = this.faceFrameSources[i].OpenReader();
                            this.faceFrameReaders[i].FrameArrived += this.faceReader_FrameArrived;
                        }
                    }
                }
                else
                {
                    this.runtime.SkeletonFrameReady -= SkeletonReady;
                    for (int i = 0; i < this.faceFrameSources.Length; i++)
                    {
                        this.faceFrameReaders[i].FrameArrived -= this.faceReader_FrameArrived;
                        this.faceFrameReaders[i].Dispose();
                        this.faceFrameSources[i].Dispose();
                    }
                }

                this.FInvalidateConnect = false;
            }

            List<FaceFrameResult> results = new List<FaceFrameResult>();


            for (int i = 0; i < lastResults.Length; i++)
            {
                if (this.lastResults[i] != null && this.faceFrameReaders[i].FaceFrameSource.IsTrackingIdValid)
                {
                    results.Add(lastResults[i]);
                }
            }

            this.FOutWearGlasses.SliceCount = results.Count;
            this.FOutUserIndex.SliceCount = results.Count;
            this.FOutSizeInfrared.SliceCount = results.Count;
            this.FOutSizeColor.SliceCount = results.Count;
            this.FOutRightEyeClosed.SliceCount = results.Count;
            this.FOutPositionInfrared.SliceCount = results.Count;
            this.FOutPositionColor.SliceCount = results.Count;
            this.FOutPointsColor.SliceCount = results.Count;
            this.FOutOrientation.SliceCount = results.Count;
            this.FOutMouthOpen.SliceCount = results.Count;
            this.FOutMouthMoved.SliceCount = results.Count;
            this.FOutlookAway.SliceCount = results.Count;
            this.FOutLeftEyeClosed.SliceCount = results.Count;
            this.FOutHappy.SliceCount = results.Count;
            this.FOutEngaged.SliceCount = results.Count;
            this.FOutPointsWorld.SliceCount = results.Count;

            for (int i = 0; i < results.Count; i++)
            {
                this.WriteFaceData(results[i], i);
            }
        }

        private Vector2 ProcessPoint(Vector2 pos)
        {
            pos.X /= 1920.0f;
            pos.X = pos.X * 2.0f - 1.0f;

            pos.Y = 1.0f - (pos.Y / 1080.0f);
            pos.Y = pos.Y * 2.0f - 1.0f;
            return pos;
        }

        private void WriteFaceData(FaceFrameResult res, int slice)
        {
            Vector2 pos;
            Vector2 size;

            size.X = Math.Abs(res.FaceBoundingBoxInColorSpace.Right - res.FaceBoundingBoxInColorSpace.Left);
            size.Y = Math.Abs(res.FaceBoundingBoxInColorSpace.Bottom - res.FaceBoundingBoxInColorSpace.Top);

            pos.X = (float)res.FaceBoundingBoxInColorSpace.Left + (size.X * 0.5f);
            pos.Y = (float)res.FaceBoundingBoxInColorSpace.Top + (size.Y * 0.5f);

            pos = this.ProcessPoint(pos);

            size.X /= 1920.0f;
            size.Y /= 1080.0f;

            this.FOutPositionColor[slice] = pos;
            this.FOutSizeColor[slice] = size;

            this.FOutPointsColor[slice].SliceCount = res.FacePointsInColorSpace.Count;
            this.FOutPointsWorld[slice].SliceCount = res.FacePointsInColorSpace.Count;
            var pointRef = this.FOutPointsColor[slice];
            var wRef = this.FOutPointsWorld[slice];
            for (int i = 0; i < res.FacePointsInColorSpace.Count; i++)
            {
                var pt = res.FacePointsInColorSpace[(FacePointType)i];
                pointRef[i] = this.ProcessPoint(new Vector2(pt.X, pt.Y));
            }



            this.FOutOrientation[slice] = new Quaternion(res.FaceRotationQuaternion.X, res.FaceRotationQuaternion.Y,
                res.FaceRotationQuaternion.Z, res.FaceRotationQuaternion.W);

            this.FOutEngaged[slice] = res.FaceProperties[FaceProperty.Engaged].ToString();
            this.FOutWearGlasses[slice] = res.FaceProperties[FaceProperty.WearingGlasses].ToString();
            this.FOutHappy[slice] = res.FaceProperties[FaceProperty.Happy].ToString();
            this.FOutLeftEyeClosed[slice] = res.FaceProperties[FaceProperty.LeftEyeClosed].ToString();
            this.FOutRightEyeClosed[slice] = res.FaceProperties[FaceProperty.RightEyeClosed].ToString();
            this.FOutlookAway[slice] = res.FaceProperties[FaceProperty.LookingAway].ToString();
            this.FOutMouthMoved[slice] = res.FaceProperties[FaceProperty.MouthMoved].ToString();
            this.FOutMouthOpen[slice] = res.FaceProperties[FaceProperty.MouthOpen].ToString();
            this.FOutUserIndex[slice] = (int)res.TrackingId;
        }

        void faceReader_FrameArrived(object sender, Microsoft.Kinect.Face.FaceFrameArrivedEventArgs e)
        {
            using (FaceFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var res = frame.FaceFrameResult;

                    if(res != null)
                    {
                        for (int i = 0; i < this.lastResults.Length; i++)
                        {
                            if (frame.TrackingId == this.faceFrameReaders[i].FaceFrameSource.TrackingId)
                            {
                                this.lastResults[i] = res;
                            }
                        }

                        this.FOutFrameNumber[0] = (int)frame.FaceFrameResult.RelativeTime.Ticks;

                        //this.WriteFaceData(res, 0);
                    }

                    
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

                    for (int i = 0; i < this.lastResults.Length;i++)
                    {
                        if (this.lastframe[i].IsTracked)
                        {
                            this.faceFrameSources[i].TrackingId = this.lastframe[i].TrackingId;
                        }
                        /*if (this.faceFrameSources[i].IsTrackingIdValid)
                        {

                        }
                        else
                        {
  
                        }*/
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
    }
}
