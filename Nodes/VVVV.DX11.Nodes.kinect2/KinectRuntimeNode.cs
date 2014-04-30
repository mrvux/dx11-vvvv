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
        [Input("Index", IsSingle = true)]
        IDiffSpread<int> FInIndex;

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

        [Output("Is Started", IsSingle = true)]
        ISpread<bool> FOutStarted;

        /*[Output("Color FOV")]
        ISpread<Vector2D> FOutColorFOV;

        [Output("Depth FOV")]
        ISpread<Vector2D> FOutDepthFOV;*/

        private KinectRuntime runtime = new KinectRuntime();

        private bool haskinect = false;

        public void Evaluate(int SpreadMax)
        {

            bool reset = false;

            if (this.FInIndex.IsChanged || this.FInReset[0] || this.runtime.Runtime == null)
            {
                this.haskinect = this.runtime.Assign(this.FInIndex[0]);
                reset = true;
            }

            if (this.haskinect)
            {

                if (this.FInEnabled.IsChanged || reset)
                {
                    if (this.FInEnabled[0])
                    {
                        this.runtime.Start(this.FInEnableColor[0], this.FInEnableSkeleton[0], this.FInDepthMode[0]);
                    }
                    else
                    {
                        this.runtime.Stop();
                    }

                    reset = true;
                }

                if (this.FInDepthMode.IsChanged || reset)
                {
                    this.runtime.SetDepthMode(this.FInDepthMode[0]);
                }


                if (this.FInEnableColor.IsChanged || reset)
                {
                    this.runtime.SetColor(this.FInEnableColor[0]);
                }

                if (this.FInInfrared.IsChanged || reset)
                {
                    this.runtime.SetInfrared(this.FInInfrared[0]);
                }

                if (this.FInEnablePlayer.IsChanged || reset)
                {
                    this.runtime.SetPlayer(this.FInEnablePlayer[0]);
                }

                if (this.FInEnableSkeleton.IsChanged || reset)
                {
                    this.runtime.EnableSkeleton(this.FInEnableSkeleton[0], false);
                }


                this.FOutStatus[0] = runtime.Runtime.IsAvailable;
                this.FOutRuntime[0] = runtime;
                this.FOutStarted[0] = runtime.IsStarted;



                /*this.FOutColorFOV.SliceCount = 1;
                this.FOutDepthFOV.SliceCount = 1;*/

                /*this.FOutColorFOV[0] = new Vector2D(this.runtime.Runtime.ColorStream.NominalHorizontalFieldOfView,
                                                    this.runtime.Runtime.ColorStream.NominalVerticalFieldOfView) * (float)VMath.DegToCyc;

                this.FOutDepthFOV[0] = new Vector2D(this.runtime.Runtime.DepthStream.NominalHorizontalFieldOfView,
                    								this.runtime.Runtime.DepthStream.NominalVerticalFieldOfView) * (float)VMath.DegToCyc;*/
            }

            this.FOutKCnt[0] = KinectSensor.KinectSensors.Count;
        }

        public void Dispose()
        {
            if (this.runtime != null)
            {
                this.runtime.Stop();
                this.runtime.Runtime.Dispose();
            }
        }
    }
}
