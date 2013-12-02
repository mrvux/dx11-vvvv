using FlareTic.API;
using FlareTic.API.DX11;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FlareTic.Nodes.Kinect2
{
    [SceneGraphNode(Name="Kinect2",Category="Devices",SystemName="flt.sg.kinect2",ShowInList=true)]
    public class Kinect2Manager : ISceneGraphNodeInstance, IKinectManager
    {
        private IBoolParameter enabled;

        private ISceneGraphNodeContainer container;

        private KinectSensor sensor;

        private IntPtr colorread;
        private IntPtr colorwrite;
        private uint colorsize;
        private object m_colorlock = new object();

        private IntPtr depthread;
        private IntPtr depthwrite;
        private uint depthsize;
        private object m_depthlock = new object();

        private IntPtr irread;
        private IntPtr irwrite;
        private uint irsize;
        private object m_irlock = new object();

        private ColorFrameReader colorreader;
        private DepthFrameReader depthreader;
        private InfraredFrameReader irreader;

        public void AssignContainer(ISceneGraphNodeContainer container)
        {
            this.container = container;
            this.container.BindInterface<IKinectManager>(this);
            this.sensor = KinectSensor.Default;
            this.sensor.Open();

            colorreader = this.sensor.ColorFrameSource.OpenReader();
            colorreader.FrameArrived += reader_FrameArrived;
            int cs = 1920 * 1080 * 4;
            this.colorsize = (uint)cs;

            colorread = Marshal.AllocHGlobal((int)colorsize);
            colorwrite = Marshal.AllocHGlobal((int)colorsize);


            depthreader = this.sensor.DepthFrameSource.OpenReader();
            this.depthsize = depthreader.DepthFrameSource.FrameDescription.LengthInPixels * depthreader.DepthFrameSource.FrameDescription.BytesPerPixel;
            depthread = Marshal.AllocHGlobal((int)depthsize);
            depthwrite = Marshal.AllocHGlobal((int)depthsize);
            depthreader.FrameArrived += depthreader_FrameArrived;

            this.irreader = this.sensor.InfraredFrameSource.OpenReader();
            this.irsize = irreader.InfraredFrameSource.FrameDescription.LengthInPixels * irreader.InfraredFrameSource.FrameDescription.BytesPerPixel;
            irread = Marshal.AllocHGlobal((int)irsize);
            irwrite = Marshal.AllocHGlobal((int)irsize);
            irreader.FrameArrived += irread_FrameArrived;

        }

        void irread_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
                using (frame)
                {
                    lock (m_irlock)
                    {
                        frame.CopyFrameDataToBuffer(irsize, this.irwrite);
                        IntPtr swap = this.irread;
                        this.irread = this.irwrite;
                        this.irwrite = swap;
                    }

                    if (this.NewIRFrame != null)
                    {
                        this.NewIRFrame(this, new EventArgs());
                    }
                }
            }
        }

        void depthreader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
                using (frame)
                {
                    lock (m_depthlock)
                    {
                        frame.CopyFrameDataToBuffer(depthsize, this.depthwrite);

                        IntPtr swap = this.depthread;
                        this.depthread = this.depthwrite;
                        this.depthwrite = swap;
                    }

                    if (this.NewDepthFrame != null)
                    {
                        this.NewDepthFrame(this, new EventArgs());
                    }
                }
            }
        }

        void reader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
                using (frame)
                {
                    lock (m_colorlock)
                    {
                        frame.CopyConvertedFrameDataToBuffer(colorsize, this.colorwrite, ColorImageFormat.Bgra);

                        IntPtr swap = this.colorread;
                        this.colorread = this.colorwrite;
                        this.colorwrite = swap;
                    }

                    if (this.NewColorFrame != null)
                    {
                        this.NewColorFrame(this, new EventArgs());
                    }
                }
            }


        }

        public void Dispose()
        {
            
        }

        public IntPtr ColorFrame
        {
            get { return this.colorread; }
        }

        public event EventHandler NewColorFrame;


        public IntPtr DepthFrame
        {
            get { return this.depthread; }
        }

        public event EventHandler NewDepthFrame;


        public int DepthSize
        {
            get { return (int)this.depthsize; }
        }


        public IntPtr IRFrame
        {
            get { return this.irread; }
        }

        public event EventHandler NewIRFrame;
    }
}
