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
    [PluginInfo(Name = "Kinect", 
	            Category = "Devices", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11",
	            Help = "Provides access to a Kinect through the MSKinect API")]
    public class KinectRuntimeNode : IPluginEvaluate, IDisposable
    {
        [Input("Motor Angle", IsSingle = true, DefaultValue = 0.5)]
        protected IDiffSpread<double> FInAngle;

        [Input("Emit Infrared", IsSingle = true, DefaultValue = 1)]
        protected IDiffSpread<bool> FInInfraredEmit;

        [Input("Index", IsSingle = true)]
        protected IDiffSpread<int> FInIndex;

        [Input("Enable Color", IsSingle = true)]
        protected IDiffSpread<bool> FInEnableColor;

        [Input("Infrared Color", IsSingle = true)]
        protected IDiffSpread<bool> FInInfrared;

        [Input("Enable Depth", IsSingle = true)]
        protected IDiffSpread<bool> FInDepthMode;

        [Input("High Res Depth", IsSingle = true)]
        protected IDiffSpread<bool> FInDeptRes;

        [Input("Depth Range", IsSingle = true)]
        protected IDiffSpread<DepthRange> FInDepthRange;

        [Input("Enable Skeleton", IsSingle = true)]
        protected IDiffSpread<bool> FInEnableSkeleton;

        [Input("Enable Skeleton Smoothing", IsSingle = true, DefaultValue = 1)]
        protected IDiffSpread<bool> FInEnableSmooth;

        [Input("Skeleton Mode", IsSingle = true)]
        protected IDiffSpread<SkeletonTrackingMode> FInSkMode;

        [Input("Smooth Parameters", IsSingle = true)]
        protected Pin<TransformSmoothParameters> FSmoothParams;

        [Input("Enabled", IsSingle = true)]
        protected IDiffSpread<bool> FInEnabled;

        [Input("Reset", IsBang = true)]
        protected ISpread<bool> FInReset;

        [Output("Kinect Runtime", IsSingle = true)]
        protected ISpread<KinectRuntime> FOutRuntime;

        [Output("Kinect Count", IsSingle = true)]
        protected ISpread<int> FOutKCnt;

        [Output("Kinect Status", IsSingle = true)]
        protected ISpread<KinectStatus> FOutStatus;

        [Output("Is Started", IsSingle = true)]
        protected ISpread<bool> FOutStarted;

        [Output("Color FOV")]
        protected ISpread<Vector2D> FOutColorFOV;

        [Output("Depth FOV")]
        protected ISpread<Vector2D> FOutDepthFOV;

        [Output("Accelerometer")]
        protected ISpread<Vector4D> FOutAccelerometer;

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

                if (this.FInDepthMode.IsChanged || reset || this.FInDeptRes.IsChanged)
                {
                    this.runtime.SetDepthMode(this.FInDepthMode[0], this.FInDeptRes[0]);
                }


                if (this.FInEnableColor.IsChanged || this.FInInfrared.IsChanged || reset)
                {
                    this.runtime.SetColor(this.FInEnableColor[0],this.FInInfrared[0]);
                }

                if (this.FInInfraredEmit.IsChanged || reset)
                {
                    try
                    {
                        this.runtime.Runtime.ForceInfraredEmitterOff = !this.FInInfraredEmit[0];
                    }
                    catch { }
                }



                if (this.FInDepthRange.IsChanged || reset)
                {
                    try
                    {
                        this.runtime.SetDepthRange(this.FInDepthRange[0]);
                    }
                    catch { }
                }




                if (this.FInEnableSkeleton.IsChanged || reset)
                {
                    TransformSmoothParameters sp;

                        sp = this.runtime.DefaultSmooth();
                    this.runtime.EnableSkeleton(this.FInEnableSkeleton[0], this.FInEnableSmooth[0], sp);
                }

                if (this.FInSkMode.IsChanged || reset)
                {
                    this.runtime.SetSkeletonMode(this.FInSkMode[0]);
                }


                if (this.FInAngle.IsChanged || reset)
                {
                    if (this.runtime.IsStarted)
                    {
                        try { this.runtime.Runtime.ElevationAngle = (int)VMath.Map(this.FInAngle[0], 0, 1, this.runtime.Runtime.MinElevationAngle, this.runtime.Runtime.MaxElevationAngle, TMapMode.Clamp); }
                        catch { }
                    }
                }

                this.FOutStatus[0] = runtime.Runtime.Status;
                this.FOutRuntime[0] = runtime;
                this.FOutStarted[0] = runtime.IsStarted;

                Vector4 va = this.runtime.Runtime.AccelerometerGetCurrentReading();
                Vector4D acc = new Vector4D(va.X, va.Y, va.Z, va.W);
                

                this.FOutColorFOV.SliceCount = 1;
                this.FOutDepthFOV.SliceCount = 1;
                this.FOutAccelerometer.SliceCount = 1;

                this.FOutAccelerometer[0] = acc;

                this.FOutColorFOV[0] = new Vector2D(this.runtime.Runtime.ColorStream.NominalHorizontalFieldOfView,
                                                    this.runtime.Runtime.ColorStream.NominalVerticalFieldOfView) * (float)VMath.DegToCyc;

                this.FOutDepthFOV[0] = new Vector2D(this.runtime.Runtime.DepthStream.NominalHorizontalFieldOfView,
                    								this.runtime.Runtime.DepthStream.NominalVerticalFieldOfView) * (float)VMath.DegToCyc;
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
