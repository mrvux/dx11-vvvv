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
using PreposeGestures;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "GestureStatus",
                Category = "Kinect2",
                Version = "Microsoft",
                Author = "flateric",
                Tags = "DX11",
                Help = "Gets kinect prepose gesture status")]
    public unsafe class KinectPreposeGestureStatusNode : IPluginEvaluate
    {

        [Input("Status")]
        protected Pin<GestureStatus> FInStatus;

        [Output("Gesture Name")]
        protected ISpread<string> FOutName;

        [Output("Step Names")]
        protected ISpread<ISpread<string>> FOutSteps;

        [Output("Confidence")]
        protected ISpread<double> FOutConfidence;

        [Output("Distance")]
        protected ISpread<double> FOutDistance;

        [Output("Completed Count")]
        protected ISpread<int> FOutCompleCount;

        [Output("Current Step")]
        protected ISpread<int> FOutStepIndex;

        public KinectPreposeGestureStatusNode()
        {

        }

        public void Evaluate(int SpreadMax)
        {
            if (SpreadMax > 0 && this.FInStatus.IsConnected && this.FInStatus[0] != null)
            {
                int cnt = this.FInStatus.SliceCount;

                this.FOutName.SliceCount = cnt;
                this.FOutConfidence.SliceCount = cnt;
                this.FOutStepIndex.SliceCount = cnt;
                this.FOutSteps.SliceCount = cnt;
                this.FOutDistance.SliceCount = cnt;
                this.FOutCompleCount.SliceCount = cnt;

                for (int i = 0; i < this.FInStatus.SliceCount; i++)
                {
                    var gesture = this.FInStatus[0];

                    this.FOutName[i] = gesture.GestureName;
                    this.FOutConfidence[i] = gesture.confidence;
                    this.FOutStepIndex[i] = gesture.CurrentStep;
                    this.FOutDistance[i] = gesture.Distance;
                    this.FOutCompleCount[i] = gesture.CompletedCount;

                    this.FOutSteps[i].SliceCount = gesture.NumSteps;
                    for (int j = 0; j < gesture.NumSteps; j++)
                    {
                        this.FOutSteps[i][j] = gesture.StepNames[j];
                    }
                }
            }
            else
            {
                this.FOutName.SliceCount = 0;
                this.FOutConfidence.SliceCount = 0;
                this.FOutStepIndex.SliceCount = 0;
                this.FOutSteps.SliceCount = 0;
                this.FOutDistance.SliceCount = 0;
                this.FOutCompleCount.SliceCount = 0;
            }
        }

    }
}
