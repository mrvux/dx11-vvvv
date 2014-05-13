using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Kinect;

namespace VVVV.MSKinect.Lib
{
    public class KinectRuntime
    {
        public KinectSensor Runtime { get; private set; }

        public bool IsStarted { get; private set; }

        public event EventHandler<BodyFrameArrivedEventArgs> SkeletonFrameReady;
        public event EventHandler<ColorFrameArrivedEventArgs> ColorFrameReady;
        public event EventHandler<DepthFrameArrivedEventArgs> DepthFrameReady;
        public event EventHandler<InfraredFrameArrivedEventArgs> IRFrameReady;
        public event EventHandler<BodyIndexFrameArrivedEventArgs> BodyFrameReady;

        private DepthFrameReader depthreader;
        private ColorFrameReader colorreader;
        private InfraredFrameReader irreader;
        private BodyFrameReader bodyreader;
        private BodyIndexFrameReader playerreader;

        public KinectRuntime()
        {

        }

        public bool Assign(int idx)
        {
            if (this.Runtime != null)
            {
                this.Runtime = null;
            }

            if (this.IsStarted)
            {
                this.Stop();
            }

            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.Runtime = KinectSensor.KinectSensors[idx % KinectSensor.KinectSensors.Count];
                return true;
            }
            else
            {
                return false;
            }
        }



       /* void Runtime_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (this.AllFrameReady != null)
            {
                this.AllFrameReady(sender, e);
            }

        }*/

        void Runtime_DepthFrameReady(object sender, DepthFrameArrivedEventArgs e)
        {
            if (this.DepthFrameReady != null)
            {
                this.DepthFrameReady(sender, e);
            }
        }

        private void Runtime_ColorFrameReady(object sender, ColorFrameArrivedEventArgs e)
        {
            if (this.ColorFrameReady != null)
            {
                this.ColorFrameReady(sender, e);
            }
        }

        private void Runtime_SkeletonFrameReady(object sender, BodyFrameArrivedEventArgs e)
        {
            if (this.SkeletonFrameReady != null)
            {
                this.SkeletonFrameReady(sender, e);
            }
        }

        private void Runtime_PlayerFrameReady(object sender, BodyIndexFrameArrivedEventArgs e)
        {
            if (this.BodyFrameReady != null)
            {
                this.BodyFrameReady(sender, e);
            }
        }

        public void EnableSkeleton(bool enable, bool smooth)//, TransformSmoothParameters sp)
        {
            if (enable)
            {
                this.bodyreader = this.Runtime.BodyFrameSource.OpenReader();
                this.bodyreader.FrameArrived += this.Runtime_SkeletonFrameReady;
            }           
        }


        public void SetPlayer(bool enable)
        {
            if (enable)
            {
                playerreader = this.Runtime.BodyIndexFrameSource.OpenReader();
                playerreader.FrameArrived += this.Runtime_PlayerFrameReady;
            }
        }

        public void SetDepthMode(bool enable)
        {
            if (enable)
            {
                depthreader = this.Runtime.DepthFrameSource.OpenReader();
                depthreader.FrameArrived += this.Runtime_DepthFrameReady ;
            }
        }

        public void SetColor(bool enable)
        {
            colorreader = this.Runtime.ColorFrameSource.OpenReader();
            colorreader.FrameArrived += this.Runtime_ColorFrameReady;
        }

        public void SetInfrared(bool enable)
        {
            irreader = this.Runtime.InfraredFrameSource.OpenReader();
            irreader.FrameArrived += irreader_FrameArrived;
        }

        void irreader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            if (this.IRFrameReady != null)
            {
                this.IRFrameReady(sender, e);
            }
        }


        #region Start
        public void Start(bool color, bool skeleton, bool depth)
        {
            if (this.Runtime != null)
            {
                if (this.IsStarted)
                {
                    this.Stop();
                }

                if (!this.IsStarted)
                {
                    try
                    {
                        this.Runtime.Open();

                        this.IsStarted = true;
                    }
                    catch (Exception ex)
                    {
                        this.IsStarted = false;
                    }
                }

                //this.Runtime.AllFramesReady += Runtime_AllFramesReady;
            }
        }

        #endregion

        #region Stop
        public void Stop()
        {
            if (this.Runtime != null)
            {
                if (this.IsStarted)
                {
                    try
                    {

                    }
                    catch
                    {

                    }
                }

                this.IsStarted = false;
            }
        }
        #endregion
    }
}
