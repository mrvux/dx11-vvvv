using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX;
using VVVV.MSKinect.Lib;
using VVVV.PluginInterfaces.V1;
using Microsoft.Kinect;


namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "IR Pipet",
                Category = "Kinect2",
                Version = "Microsoft",
                Author = "u7angel",
                Tags = "DX11",
                Help = "pipet IR image")]
    public class KinectRawIRNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Input("Pixel")]
        public ISpread<Vector2> FInPixelPos;


        [Output("Value")]
        protected ISpread<double> FOutValue;


        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;

        private bool FInvalidate = false;

        private Body[] lastframe = new Body[6];
        private object m_lock = new object();
       

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.runtime.IRFrameReady -= IRFrameReady;
                }

                if (this.FInRuntime.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        
                        this.FInRuntime[0].IRFrameReady += IRFrameReady;
                    }

                }

                this.FInvalidateConnect = false;
            }

            if (this.FInvalidate)
            {
                if (this.lastframe != null)
                {
                    



                }
                this.FInvalidate = false;
            }
        }


        private unsafe void IRFrameReady(object sender, InfraredFrameArrivedEventArgs e)
        {
            if (e.FrameReference != null)
            {
                using (var frame = e.FrameReference.AcquireFrame())
                {
                    if (frame != null)
                    {
                        using (var buffer = frame.LockImageBuffer())
                        {
                            short* data = (short*)buffer.UnderlyingBuffer;
                            FOutValue.SliceCount = 0;

                            int pixelX = 10;
                            int pixelY = 10;

                            foreach (var item in FInPixelPos)
                            {
                                pixelX = (int)item.X;
                                pixelY = (int)item.Y;

                                pixelX = pixelX < 0 ? 0 : pixelX;
                                pixelY = pixelY < 0 ? 0 : pixelY;

                                pixelX = pixelX > 511 ? 511 : pixelX;
                                pixelY = pixelY > 423 ? 423 : pixelY;

                                double pixel = data[pixelY * 512 + pixelX];
                                FOutValue.Add(pixel);
                            }
                            
                        }
                    }
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
