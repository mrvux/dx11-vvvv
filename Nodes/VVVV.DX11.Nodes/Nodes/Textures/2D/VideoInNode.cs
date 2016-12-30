using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VideoInputSharp;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System.Runtime.InteropServices;
using System.Threading;

namespace VVVV.DX11.Nodes
{
    public unsafe class VideoInThread
    {
        private Thread thr;

        private object m_lock = new object();
        private bool m_bRunning = false;

        VideoInputSharp.Capture capture;

        private IntPtr rgbbuffer = IntPtr.Zero;
        private IntPtr buffer0 = IntPtr.Zero;
        private IntPtr buffer1 = IntPtr.Zero;

        private int width;
        private int height;
        private int pixcount;
        public int size;

        public event EventHandler OnFrameReady;

        public IntPtr frontBuffer { get { return this.buffer1; } }

        public void Start(int w, int h,int fps)
        {
            this.capture = new Capture();
            this.capture.Open(0, w, h, fps);

            this.width = this.capture.GetWidth();
            this.height = this.capture.GetHeight();
            this.pixcount = this.width * this.height;

            this.rgbbuffer = Marshal.AllocCoTaskMem(this.width * this.height * 3);
            this.buffer0 = Marshal.AllocCoTaskMem(this.width * this.height * 4);
            this.buffer1 = Marshal.AllocCoTaskMem(this.width * this.height * 4);
            this.size = this.width * this.height * 4;

            if (this.m_bRunning)
            {
                return;
            }



            this.thr = new Thread(new ThreadStart(this.Run));
            this.m_bRunning = true;
            this.thr.Start();
        }

        public void Stop()
        {
            this.m_bRunning = false;
        }

        public int GetWidth()
        {
            return this.width;
        }

        public int GetHeight()
        {
            return this.height;
        }

        private void Run()
        {
            while (this.m_bRunning)
            {
                this.capture.GetPixels(this.rgbbuffer);

                
                byte* brgb = (byte*)this.rgbbuffer.ToPointer();
                byte* brgba = (byte*)this.buffer0.ToPointer();

                for (int i = 0; i < this.pixcount; i++)
                {
                    brgba[i * 4] = brgb[i * 3];
                    brgba[i * 4 + 1] = brgb[i * 3 + 1];
                    brgba[i * 4 + 2] = brgb[i * 3 + 2];
                    brgba[i * 4 + 3] = 255;
                }

                IntPtr temp = this.buffer0;
                this.buffer0 = this.buffer1;
                this.buffer1 = temp;


                if (this.OnFrameReady != null)
                {
                    this.OnFrameReady(this, new EventArgs());
                }
            }

            this.capture.Close();

            Marshal.FreeCoTaskMem(this.buffer0);
            Marshal.FreeCoTaskMem(this.buffer1);
            Marshal.FreeCoTaskMem(this.rgbbuffer);

        }
    }


    [PluginInfo(Name="VideoIn",Category="DX11.Texture", Version="DShow")]
    public unsafe class VideoInNode : IPluginEvaluate, IDX11ResourceProvider, IDisposable
    {
        [Input("Width", DefaultValue =640)]
        IDiffSpread<int> FInW;

        [Input("Height", DefaultValue = 480)]
        IDiffSpread<int> FInH;

        [Input("Fps", DefaultValue = 30)]
        IDiffSpread<int> FInFPS;

        [Input("Device Id", Order = 500, MinValue = 0)]
        IDiffSpread<int> FInDeviceId;

        [Input("Reset", Order = 505, IsBang=true)]
        IDiffSpread<bool> FInReset;

        [Input("Enabled", Order = 501, MinValue = 0)]
        IDiffSpread<bool> FInEnabled;

        [Output("Texture Out", IsSingle = true)]
        protected Pin<DX11Resource<DX11DynamicTexture2D>> FTextureOutput;

        [Output("Width Out", DefaultValue = 640)]
        ISpread<int> FOutW;

        [Output("Height Out", DefaultValue = 480)]
        ISpread<int> FOutH;

        private VideoInThread videoin;

        bool invalidate = false;
        bool reset = false;

        public void Evaluate(int SpreadMax)
        {
            if (this.videoin == null || this.FInReset[0])
            {
                if (this.videoin != null)
                {
                    this.videoin.OnFrameReady -= videoin_OnFrameReady;
                    this.videoin.Stop();
                }

                this.reset = true;
                this.videoin = new VideoInThread();
                this.videoin.OnFrameReady += this.videoin_OnFrameReady;
                this.videoin.Start(this.FInW[0], this.FInH[0],this.FInFPS[0]);
            }

            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11DynamicTexture2D>();
            }
               try
                {
                    this.FOutW[0] = this.videoin.GetWidth();
                    this.FOutH[0] = this.videoin.GetHeight();
                }
                catch
                {

                }
            
            
        }

        void videoin_OnFrameReady(object sender, EventArgs e)
        {
            this.invalidate = true;
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (this.reset)
            {
                if (this.FTextureOutput[0].Contains(context))
                {
                    this.FTextureOutput[0].Dispose(context);
                }

                DX11DynamicTexture2D t = new DX11DynamicTexture2D(context, this.videoin.GetWidth(), this.videoin.GetHeight(), SlimDX.DXGI.Format.B8G8R8A8_UNorm);
                this.FTextureOutput[0][context] = t;
                this.reset = false;
            }

            if (this.invalidate)
            {
                this.FTextureOutput[0][context].WriteData(this.videoin.frontBuffer, this.videoin.size);
                this.invalidate = false;
            }
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            this.FTextureOutput[0].Dispose(context);
        }

        public void Dispose()
        {
            this.FTextureOutput[0].Dispose();
            if (this.videoin != null)
            {
                this.videoin.Stop();
            }
        }
    }
}
