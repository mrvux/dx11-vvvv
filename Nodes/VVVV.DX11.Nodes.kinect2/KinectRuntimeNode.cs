using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.MSKinect.Lib;
using VVVV.Utils.VMath;
using Microsoft.Kinect;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "Kinect2", 
	            Category = "Devices", 
	            Version = "Microsoft", 
	            Author = "flateric", 
	            Tags = "DX11",
	            Help = "Provides access to a Kinect through the MSKinect API")]
    public class KinectRuntimeNode : IPluginEvaluate, IDisposable
    {
        /*[Input("Index", IsSingle = true)]
        IDiffSpread<int> FInIndex;*/

        [Input("Enable Color", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FInEnableColor;

        [Input("Enable IR", IsSingle = true, DefaultValue=1)]
        IDiffSpread<bool> FInInfrared;

        [Input("Enable Depth", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FInDepthMode;

        [Input("Enable Skeleton", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FInEnableSkeleton;

        [Input("Enable Player", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FInEnablePlayer;

        [Input("Enabled", IsSingle = true)]
        IDiffSpread<bool> FInEnabled;

        [Input("Reset", IsBang = true)]
        ISpread<bool> FInReset;

        [Output("Kinect Runtime", IsSingle = true)]
        ISpread<KinectRuntime> FOutRuntime;

        [Output("Kinect Count", IsSingle = true)]
        ISpread<int> FOutKCnt;

        [Output("Is Available", IsSingle = true)]
        ISpread<bool> FOutStatus;

        [Output("Color FOV")]
        ISpread<Vector2D> FOutColorFOV;

        [Output("Depth FOV")]
        ISpread<Vector2D> FOutDepthFOV;

        [Output("Is Started", IsSingle = true)]
        ISpread<bool> FOutStarted;

        [Output("DepthRange (cm)" )]
        ISpread<Vector2D> FDepthrange;

        [Output("Intrinsics")]
        ISpread<CameraIntrinsics> intrinsics;

        private KinectRuntime runtime = new KinectRuntime();

        private bool haskinect = false;

        
        private bool onkinectreset = false;


        public void Evaluate(int SpreadMax)
        {
            bool reset = false;

            if (this.FInReset[0] || this.runtime.Runtime == null)
            {
                bool regevent = this.runtime.Runtime == null;
                //Keep until we have multiple kinect support
                this.haskinect = this.runtime.Assign(0);

                if (regevent)
                {
                    this.runtime.OnReset += (s, e) => this.onkinectreset = true;
                }

                reset = true;
            }

            if (this.haskinect)
            {

                if (this.FInEnabled.IsChanged || reset)
                {
                    if (this.FInEnabled[0])
                    {
                        this.runtime.Start();
                    }
                    else
                    {
                        this.runtime.Stop();
                    }

                    reset = true;
                }

                if (this.FInDepthMode.IsChanged || reset || onkinectreset)
                {
                    this.runtime.SetDepthMode(this.FInDepthMode[0]);
                }


                if (this.FInEnableColor.IsChanged || reset || onkinectreset)
                {
                    this.runtime.SetColor(this.FInEnableColor[0]);
                }

                if (this.FInInfrared.IsChanged || reset || onkinectreset)
                {
                    this.runtime.SetInfrared(this.FInInfrared[0]);
                }

                if (this.FInEnablePlayer.IsChanged || reset || onkinectreset)
                {
                    this.runtime.SetPlayer(this.FInEnablePlayer[0]);
                }

                if (this.FInEnableSkeleton.IsChanged || reset || onkinectreset)
                {
                    this.runtime.EnableSkeleton(this.FInEnableSkeleton[0], false);
                }

                //TODO : Modify here to make sure flag has been taken properly
                if (this.onkinectreset)
                {
                    this.onkinectreset = false;
                }  


                this.FOutStatus[0] = runtime.Runtime.IsAvailable;
                this.FOutRuntime[0] = runtime;
                this.FOutStarted[0] = runtime.IsStarted;


                this.FOutColorFOV.SliceCount = 1;
                this.FOutDepthFOV.SliceCount = 1;

                this.FOutColorFOV[0] = new Vector2D(this.runtime.Runtime.ColorFrameSource.FrameDescription.HorizontalFieldOfView,
                                                    this.runtime.Runtime.ColorFrameSource.FrameDescription.VerticalFieldOfView)  *(float)VMath.DegToCyc;

                this.FOutDepthFOV[0] = new Vector2D(this.runtime.Runtime.DepthFrameSource.FrameDescription.HorizontalFieldOfView,
                                                    this.runtime.Runtime.DepthFrameSource.FrameDescription.VerticalFieldOfView)  *(float)VMath.DegToCyc;

                this.FDepthrange[0] = new Vector2D( (double)this.runtime.Runtime.DepthFrameSource.DepthMinReliableDistance,
                                                    (double)this.runtime.Runtime.DepthFrameSource.DepthMaxReliableDistance);

                this.intrinsics[0] = this.runtime.Runtime.CoordinateMapper.GetDepthCameraIntrinsics();
            }

            this.FOutKCnt[0] = 1; // KinectSensor.KinectSensors.Count;
                       
        }
        public void Dispose()
        {
            if (this.runtime != null)
            {
                this.runtime.Stop();
            }
        }
    }
}
