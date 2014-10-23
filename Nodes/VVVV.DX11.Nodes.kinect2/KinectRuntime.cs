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

        public event EventHandler OnReset;

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

            this.Runtime = KinectSensor.GetDefault();
            this.Runtime.IsAvailableChanged += Runtime_IsAvailableChanged;
            return true;
        }

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
            if (enable && this.bodyreader == null)
            {
                this.bodyreader = this.Runtime.BodyFrameSource.OpenReader();
                this.bodyreader.FrameArrived += this.Runtime_SkeletonFrameReady;
            }  
            else
            {
                if (this.bodyreader != null)
                {
                    this.bodyreader.FrameArrived -= this.Runtime_SkeletonFrameReady;
                    this.bodyreader.Dispose();
                    this.bodyreader = null;
                }
            }
        }


        public void SetPlayer(bool enable)
        {
            if (enable && this.playerreader == null)
            {
                playerreader = this.Runtime.BodyIndexFrameSource.OpenReader();
                playerreader.FrameArrived += this.Runtime_PlayerFrameReady;
            }
            else
            {
                if(this.playerreader != null)
                {
                    this.playerreader.FrameArrived -= this.Runtime_PlayerFrameReady;
                    this.playerreader.Dispose();
                    this.playerreader = null;
                }
            }
        }

        public void SetDepthMode(bool enable)
        {
            if (enable && this.depthreader == null)
            {
                depthreader = this.Runtime.DepthFrameSource.OpenReader();
                depthreader.FrameArrived += this.Runtime_DepthFrameReady ;
            }
            else
            {
                if (this.depthreader != null)
                {
                    this.depthreader.FrameArrived -= this.Runtime_DepthFrameReady;
                    this.depthreader.Dispose(); this.depthreader = null;
                }
            }
        }

        public void SetColor(bool enable)
        {
            if (this.colorreader == null && enable)
            {
                colorreader = this.Runtime.ColorFrameSource.OpenReader();
                colorreader.FrameArrived += this.Runtime_ColorFrameReady;
            }
            else
            {
                if (this.colorreader != null)
                {
                    this.colorreader.FrameArrived -= this.Runtime_ColorFrameReady;
                    this.colorreader.Dispose();
                    this.colorreader = null;
                }
            }
            
        }

        public void SetInfrared(bool enable)
        {
            if (this.irreader == null && enable)
            {
                irreader = this.Runtime.InfraredFrameSource.OpenReader();
                irreader.FrameArrived += irreader_FrameArrived;
            }
            else
            {
                if (this.irreader != null)
                {
                    this.irreader.FrameArrived -= this.irreader_FrameArrived;
                    this.irreader.Dispose();
                    this.irreader = null;
                }
            }

        }

        void irreader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            if (this.IRFrameReady != null)
            {
                this.IRFrameReady(sender, e);
            }
        }


        #region Start
        public void Start()
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
            }
        }

        private void Runtime_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (this.OnReset != null)
            {
                this.OnReset(sender, new EventArgs());
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
                        this.EnableSkeleton(false, false);
                        this.SetColor(false);
                        this.SetDepthMode(false);
                        this.SetInfrared(false);
                        this.SetPlayer(false);

                        this.Runtime.Close();
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
