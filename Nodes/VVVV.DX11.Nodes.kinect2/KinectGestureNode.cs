using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Core.Logging;
using VVVV.MSKinect.Lib;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Nodes.Kinect2
{
    [PluginInfo(Name = "Gesture",
               Category = "Kinect2",
               Version = "Microsoft",
               Author = "vux",
               Tags = "DX11",
               Help = "Returns gesture from gbd file")]
    public class KinectGestureNode : IPluginEvaluate, IPluginConnections
    {
        [Import]
        protected ILogger logger;

        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Input("Gesture File", IsSingle = true, StringType = StringType.Filename)]
        protected IDiffSpread<string> gesturefile;

        [Input("Use Manual Index", IsSingle = true)]
        protected IDiffSpread<bool> manualID;

        [Input("Manual Index", IsSingle = true)]
        protected IDiffSpread<string> manualIndex;

        [Output("Gesture Names")]
        protected ISpread<string> gesturenames;

        [Output("Gesture Type")]
        protected ISpread<GestureType> gesturetype;

        [Output("Tracking Id Valid")]
        protected ISpread<bool> trackingidvalid;

        [Output("Tracking Active")]
        protected ISpread<bool> trackingActive;

        [Output("Gesture Detected")]
        protected ISpread<bool> gesturedetected;

        [Output("Gesture Confidence")]
        protected ISpread<double> gestureconfidence;

        [Output("Gesture Progress")]
        protected ISpread<double> gestureprogress;


        private bool FInvalidateConnect = false;
        private KinectRuntime runtime;
        private VisualGestureBuilderDatabase database;

        private VisualGestureBuilderFrameSource vgbFrameSource = null;
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        private Body[] lastframe = new Body[6];

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.runtime.SkeletonFrameReady -= SkeletonReady;
                    if (this.vgbFrameReader != null)
                    {
                        this.vgbFrameReader.FrameArrived -= vgbFrameReader_FrameArrived;
                        this.vgbFrameReader.Dispose();
                        this.vgbFrameReader = null;
                    }
                    if (this.vgbFrameSource != null)
                    {
                        this.vgbFrameSource.Dispose();
                        this.vgbFrameSource = null;
                    }
                }

                if (this.FInRuntime.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        this.vgbFrameSource = new VisualGestureBuilderFrameSource(this.runtime.Runtime, 0);
                        this.vgbFrameReader = this.vgbFrameSource.OpenReader();
                        this.vgbFrameReader.FrameArrived += vgbFrameReader_FrameArrived;
                        this.runtime.SkeletonFrameReady += SkeletonReady;
                    }

                }
                this.FInvalidateConnect = false;
            }

            if (this.gesturefile.IsChanged)
            {
                string s = this.gesturefile[0];

                this.Reset();

                try
                {
                    this.database = new VisualGestureBuilderDatabase(s);

                    this.gesturenames.SliceCount = (int)this.database.AvailableGesturesCount;
                    this.gesturetype.SliceCount = this.gesturenames.SliceCount;
                    this.gesturedetected.SliceCount = this.gesturenames.SliceCount;
                    this.gestureconfidence.SliceCount = this.gesturenames.SliceCount;
                    this.gestureprogress.SliceCount = this.gesturenames.SliceCount;

                    int cnt = 0;
                    foreach (Gesture g in database.AvailableGestures)
                    {
                        this.gesturenames[cnt] = g.Name;
                        this.gesturetype[cnt] = g.GestureType;
                        this.vgbFrameSource.AddGesture(g);
                        cnt++;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Log(ex);
                }
            }

            if (this.vgbFrameSource != null)
            {
                this.trackingidvalid[0] = this.vgbFrameSource.IsTrackingIdValid;
                this.trackingActive[0] = this.vgbFrameSource.IsActive;
            }
            else
            {
                this.trackingidvalid[0] = false;
                this.trackingActive[0] = false;
            }

        }

        void vgbFrameReader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    // get the discrete gesture results which arrived with the latest frame
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;

                    if (discreteResults != null)
                    {
                        int i = 0;
                        // we only have one gesture in this source object, but you can get multiple gestures
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    this.gesturedetected[i] = result.Detected;
                                    this.gestureconfidence[i] = result.Confidence;
                                }
                            }
                            i++;
                        }
                    }

                    IReadOnlyDictionary<Gesture, ContinuousGestureResult> continuousResults = frame.ContinuousGestureResults;

                    if (continuousResults != null)
                    {
                        int i = 0;
                        // we only have one gesture in this source object, but you can get multiple gestures
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.GestureType == GestureType.Continuous)
                            {
                                ContinuousGestureResult result = null;
                                continuousResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    this.gestureprogress[i] = result.Progress;
                                }
                            }
                            i++;
                        }
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
                    skeletonFrame.Dispose();
                }
            }

            if (this.manualID[0])
            {

                ulong search = 0;
                bool found = false;
                if (this.manualIndex.SliceCount > 0)
                {
                    if (ulong.TryParse(this.manualIndex[0], out search))
                    {
                        for (int i = 0; i < this.lastframe.Length; i++)
                        {
                            if (this.lastframe[i] != null && this.lastframe[i].IsTracked && this.lastframe[i].TrackingId == search)
                            {
                                found = true;
                            }
                        }
                    }
                }


                if (found)
                {
                    this.vgbFrameSource.TrackingId = search;
                }
                this.vgbFrameReader.IsPaused = found == false;
            }
            else
            {
                ulong found = 0;
                for (int i = 0; i < this.lastframe.Length; i++)
                {
                    if (this.lastframe[i] != null && this.lastframe[i].IsTracked)
                    {
                        found = this.lastframe[i].TrackingId;
                    }
                }

                if (found > 0)
                {
                    this.vgbFrameSource.TrackingId = found;
                }
                this.vgbFrameReader.IsPaused = found == 0;
            }


        }

        private void Reset()
        {
            if (database != null)
            {
                this.database.Dispose();
                this.database = null;
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
