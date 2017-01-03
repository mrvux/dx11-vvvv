using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using VVVV.MSKinect.Lib;
using Microsoft.Kinect.Toolkit.Interaction;
using Microsoft.Kinect;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using System.IO;

namespace VVVV.DX11.Nodes.Nodes
{
    public class InteractionClientTest : IInteractionClient
    {
        public InteractionInfo GetInteractionInfoAtLocation(int skeletonTrackingId, InteractionHandType handType, double x, double y)
        {
            InteractionInfo info = new InteractionInfo();
            info.IsGripTarget = false;
            info.IsPressTarget = false;
            info.PressTargetControlId = 2;
            info.PressAttractionPointX = x;
            info.PressAttractionPointY = y;

            return info;
        }
    }

    [PluginInfo(Name="Interaction",Category="Kinect",Version="Microsoft",Author="vux")]
    public class KinectMSInteractionNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Output("User Info")]
        protected ISpread<UserInfo> FOutUI;

        [Output("Skeleton Id")]
        protected ISpread<int> FOutSkelId;

        private bool FInvalidateConnect = false;

        private InteractionStream stream;
        private KinectRuntime runtime;

        private UserInfo[] infos = new UserInfo[InteractionStream.FrameUserInfoArrayLength];

        [Import()]
        protected ILogger log;

        public KinectMSInteractionNode()
        {
            Register();
        }

        private static bool registered = false;

        private static void Register()
        {
            if (!registered)
            {
                string path = Path.GetDirectoryName(typeof(KinectMSInteractionNode).Assembly.Location);

                string varpath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process);
                varpath += ";" + path;
                Environment.SetEnvironmentVariable("Path", varpath, EnvironmentVariableTarget.Process);

                registered = true;
            }
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    stream = new InteractionStream(this.FInRuntime[0].Runtime, new InteractionClientTest());
                    stream.InteractionFrameReady += stream_InteractionFrameReady;
                    this.runtime = this.FInRuntime[0];
                    this.runtime.SkeletonFrameReady += runtime_SkeletonFrameReady;
                    this.runtime.DepthFrameReady += this.runtime_DepthFrameReady;
                }
                else
                {
                    if (stream != null) 
                    {
                        this.runtime.SkeletonFrameReady -= runtime_SkeletonFrameReady;
                        this.runtime.DepthFrameReady -= this.runtime_DepthFrameReady;
                        stream.InteractionFrameReady -= stream_InteractionFrameReady;
                        stream.Dispose(); 
                        stream = null;
                        this.runtime = null;
                    }
                }
                 
                
                this.FInvalidateConnect = false;
            }

            List<UserInfo> infs = new List<UserInfo>();

            for (int i = 0; i < this.infos.Length;i++)
            {
                if (this.infos[i] != null)
                {
                    if (this.infos[i].SkeletonTrackingId != 0)
                    {
                        infs.Add(this.infos[i]);
                    }
                }
            }

            this.FOutSkelId.SliceCount = infs.Count;
            this.FOutUI.SliceCount = infs.Count;

            for (int i = 0; i < infs.Count; i++)
            {
                UserInfo ui = infs[i];
                this.FOutSkelId[i] = ui.SkeletonTrackingId;
                this.FOutUI[i] = ui;
            }
        }

        void runtime_SkeletonFrameReady(object sender, Microsoft.Kinect.SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skels = null;
            long ts = 0;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skels = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skels);
                    ts = skeletonFrame.Timestamp;
                }
            }

            if (skels != null)
            {
                Vector4 accel = this.runtime.Runtime.AccelerometerGetCurrentReading();
                stream.ProcessSkeleton(skels, accel, ts);
            }
        }

        void runtime_DepthFrameReady(object sender, Microsoft.Kinect.DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    try
                    {
                        // Hand data to Interaction framework to be processed
                        this.stream.ProcessDepth(frame.GetRawPixelData(), frame.Timestamp);
                    }
                    catch
                    {
                        // DepthFrame functions may throw when the sensor gets
                        // into a bad state.  Ignore the frame in that case.
                    }
                }
            }
        }

        void stream_InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using (InteractionFrame interactionFrame = e.OpenInteractionFrame())
            {
                if (interactionFrame != null)
                {
                    interactionFrame.CopyInteractionDataTo(this.infos);
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
