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
    [PluginInfo(Name = "Prepose",
                Category = "Kinect2",
                Version = "Microsoft",
                Author = "flateric",
                Tags = "DX11",
                Help = "Returns high definition face data for user")]
    public unsafe class KinectPreposeGestureNode : IPluginEvaluate, IPluginConnections, IDisposable
    {

        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Input("Pose String")]
        protected IDiffSpread<string> FInScript;

        [Input("Tracking Id")]
        protected ISpread<string> FInId;

        [Input("Is Paused")]
        protected ISpread<bool> FInPaused;

        [Input("Precision", DefaultValue=0.3, MinValue =0, MaxValue =1)]
        protected ISpread<double> FInPrecision;

        [Output("Is valid")]
        protected ISpread<bool> FOutValid;

        [Output("Gestures")]
        protected ISpread<string> FOutGestures;

        [Output("Is Paused")]
        protected ISpread<bool> FOutPaused;

        [Output("Gestures Status")]
        protected ISpread<GestureStatus> FOutStatus;

        [Output("Tracking Id")]
        protected ISpread<string> FOuTrackingId;


        private bool FInvalidateConnect = false;

        private PreposeGestures.PreposeGesturesFrameSource source;
        private PreposeGestures.PreposeGesturesFrameReader reader;

        private KinectRuntime runtime;

        private object m_lock = new object();


        public KinectPreposeGestureNode()
        {
            GestureStatistics.synthTimes = new List<StatisticsEntrySynthesize>();
            GestureStatistics.matchTimes = new List<StatisticsEntryMatch>();
        }

        public void Evaluate(int SpreadMax)
        {


            if (this.FInvalidateConnect)
            {
                if (this.FInRuntime.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        this.source = new PreposeGestures.PreposeGesturesFrameSource(this.runtime.Runtime, 0);
                        this.reader = this.source.OpenReader();
                        this.reader.FrameArrived += Reader_FrameArrived;
                        this.reader.IsPaused = true;
                    }
                }
                else
                {
                    //this.runtime.SkeletonFrameReady -= SkeletonReady;
                    this.reader.FrameArrived -= this.Reader_FrameArrived;
                    this.source.Dispose();

                }

                this.FInvalidateConnect = false;
            }

            if (this.source != null)
            {
                ulong id = 0;
                try
                {
                    id = ulong.Parse(this.FInId[0]);
                }
                catch
                {

                }
                this.source.TrackingId = id;
                this.reader.IsPaused = this.FInPaused[0];
            }

            if (this.FInScript.IsChanged)
            {

                try
                {
                    string str = this.FInScript[0];
                    var app = PreposeGestures.App.ReadAppText(str);

                    this.FOutGestures.SliceCount = app.Gestures.Count;
                    this.FOutGestures.AssignFrom(app.Gestures.Select(gs => gs.Name));



                    this.source.Gestures.Clear();
                    foreach (Gesture g in app.Gestures)
                    {
                        this.source.AddGesture(g);
                    }

                    this.FOutValid[0] = true;
                }
                catch
                {
                    this.FOutValid[0] = false;
                    this.source.Gestures.Clear();
                } 
            }

            if (this.source != null)
            {
                this.source.myMatcher.Precision = Convert.ToInt32(this.FInPrecision[0] * 100.0);

            }
            this.FOutPaused[0] = this.reader != null ? this.reader.IsPaused : true;
            this.FOuTrackingId[0] = this.source.TrackingId.ToString();
        }

        private void Reader_FrameArrived(object sender, PreposeGestures.PreposeGesturesFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();

            if (frame != null && frame.results != null)
            {
                this.FOutStatus.AssignFrom(frame.results);
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

        public void Dispose()
        {

        }
    }
}
