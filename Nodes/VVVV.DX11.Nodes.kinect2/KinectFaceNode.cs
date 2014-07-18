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
        private Pin<KinectRuntime> FInRuntime;

        [Output("Position Infrared")]
        private ISpread<Vector2> FOutPositionInfrared;

        [Output("Size Infrared")]
        private ISpread<Vector2> FOutSizeInfrared;

        [Output("Position Color")]
        private ISpread<Vector2> FOutPositionColor;

        [Output("Size Color")]
        private ISpread<Vector2> FOutSizeColor;

        [Output("Orientation")]
        private ISpread<Quaternion> FOutOrientation;

        [Output("Frame Number", IsSingle = true)]
        private ISpread<int> FOutFrameNumber;

        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;
        private Microsoft.Kinect.Face.FaceFrameSource faceSrc;
        private Microsoft.Kinect.Face.FaceFrameReader faceReader;
        /*private Microsoft.Kinect.Face.HighDefinitionFaceFrameSource hdSrc;
        private Microsoft.Kinect.Face.HighDefinitionFaceFrameReader hdRead;*/

        private bool FInvalidate = false;

        private object m_lock = new object();
        private int frameid = -1;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        faceSrc = new Microsoft.Kinect.Face.FaceFrameSource(this.runtime.Runtime);
                        faceSrc.FaceFrameFeatures = FaceFrameFeatures.BoundingBoxInInfraredSpace | FaceFrameFeatures.BoundingBoxInColorSpace;
                        faceReader = faceSrc.OpenReader();
                        faceReader.FrameArrived += this.faceReader_FrameArrived;

                       /* hdSrc = new HighDefinitionFaceFrameSource(this.runtime.Runtime);
                        hdRead = hdSrc.OpenReader();
                        hdRead.FrameArrived += hdRead_FrameArrived;*/
                    }
                }
                else
                {
                    if (faceSrc != null)
                    {
                        faceReader.FrameArrived -= faceReader_FrameArrived;
                        faceReader.Dispose();
                    }
                }

                this.FInvalidateConnect = false;
            }
        }

        void hdRead_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
           
        }

        void faceReader_FrameArrived(object sender, Microsoft.Kinect.Face.FaceFrameArrivedEventArgs e)
        {
            using (FaceFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var res = frame.FaceFrameResult;
                    this.FOutFrameNumber[0] = (int)frame.FaceFrameResult.RelativeTime.Ticks;
                    this.FOutOrientation[0] = new Quaternion(res.FaceRotationQuaternion.X, res.FaceRotationQuaternion.Y, res.FaceRotationQuaternion.Z, res.FaceRotationQuaternion.W);
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
