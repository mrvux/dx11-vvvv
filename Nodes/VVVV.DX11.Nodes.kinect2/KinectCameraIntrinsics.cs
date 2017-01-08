#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using Microsoft.Kinect;
using VVVV.MSKinect.Lib;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.DX11.Nodes.MSKinect
{
    #region PluginInfo
    [PluginInfo(Name = "CameraIntrinsics", 
                Category = "Kinect2", 
                Version = "Microsoft", 
                Help = "Returns Kinect Depth camera intrinsics", 
                Tags = "intrinsics, depth, kinect", 
                Author = "id144")]
    #endregion PluginInfo

    public class KinectCameraIntrinsicsNode : IPluginEvaluate 
    {
        #region fields & pins

        [Input("Input")]
        protected ISpread<CameraIntrinsics> FInputIntrinsics;

        [Output("FocalLengthX")]
        public ISpread<double> FOutputFocalLengthX;

        [Output("FocalLengthY")]
        public ISpread<double> FocalLengthY;

        [Output("PrincipalPointX")]
        public ISpread<double> PrincipalPointX;

        [Output("PrincipalPointY")]
        public ISpread<double> PrincipalPointY;

        [Output("RadialDistortionFourthOrder")]
        public ISpread<double> RadialDistortionFourthOrder;

        [Output("RadialDistortionSecondOrder")]
        public ISpread<double> RadialDistortionSecondOrder;

        [Output("RadialDistortionSixthOrder")]
        public ISpread<double> RadialDistortionSixthOrder;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins

        public void Evaluate(int SpreadMax)
        {
            if (FInputIntrinsics.SliceCount != 0)
            {
                FOutputFocalLengthX[0] = FInputIntrinsics[0].FocalLengthX;
                FocalLengthY[0] = FInputIntrinsics[0].FocalLengthY;
                PrincipalPointX[0] = FInputIntrinsics[0].PrincipalPointX;
                PrincipalPointY[0] = FInputIntrinsics[0].PrincipalPointY;
                RadialDistortionFourthOrder[0] = FInputIntrinsics[0].RadialDistortionFourthOrder;
                RadialDistortionSecondOrder[0] = FInputIntrinsics[0].RadialDistortionSecondOrder;
                RadialDistortionSixthOrder[0] = FInputIntrinsics[0].RadialDistortionSixthOrder;
            }
            else
            {
                FOutputFocalLengthX = null;
                FocalLengthY = null;
                PrincipalPointX = null;
                PrincipalPointY = null;
                RadialDistortionFourthOrder = null;
                RadialDistortionSecondOrder = null;
                RadialDistortionSixthOrder = null;
            }            
        }
    }
}
