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

namespace VVVV.DX11.Nodes
{
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

        VideoInputSharp.Capture c;

        bool invalidate = true;
        bool copyframe = false;

        private IntPtr data = IntPtr.Zero;
        private IntPtr rgbadata = IntPtr.Zero;
        private long size;
        private int pixcount;


        public void Evaluate(int SpreadMax)
        {
            this.copyframe = false;
            if (c == null || this.FInReset[0])
            {
                if (c != null)
                {
                    c.Close();
                }

                if (this.data != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(this.data);
                }

                c = new Capture();
                c.Open(this.FInDeviceId[0], this.FInFPS[0], this.FInW[0], this.FInH[0]);

                this.size = this.FInW[0] * this.FInH[0] * 4;
                this.pixcount = this.FInW[0] * this.FInH[0];


                this.data = Marshal.AllocCoTaskMem(this.FInW[0] * this.FInH[0] * 3);
                this.rgbadata = Marshal.AllocCoTaskMem((int)size);
                this.invalidate = true;

            }

            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11DynamicTexture2D>();
            }

            if (c != null && this.FInEnabled[0])
            {
                this.c.GetPixels(this.data);

                byte* brgb = (byte*) this.data.ToPointer();
                byte* brgba = (byte*)this.rgbadata.ToPointer();

                /*int cnt = 0;
                int cnta = 0;
                for (int i = 0; i < this.FInH[0]; i++)
                {
                    for (int j = 0; j < this.FInW[0]; j++)
                    {
                        brgba[cnta] = brgb[cnt];
                        brgba[cnta + 1] = brgb[cnt + 1];
                        brgba[cnta + 2] = brgb[cnt + 2];

                        cnta += 4;
                        cnt += 3;
                    }
                }*/

                for (int i = 0; i < this.pixcount; i++)
                {
                    brgba[i * 4] = brgb[i * 3];
                    brgba[i * 4 + 1] = brgb[i * 3 + 1];
                    brgba[i * 4 + 2] = brgb[i * 3 + 2];
                }

                this.copyframe = true;

                try
                {
                    this.FOutW[0] = c.GetWidth();
                    this.FOutH[0] = c.GetHeight();
                }
                catch
                {

                }
            }
            
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (c != null)
            {
                if (this.invalidate || !this.FTextureOutput[0].Contains(context))
                {
                    try
                    {
                        if (this.FTextureOutput[0].Contains(context))
                        {
                            this.FTextureOutput[0].Dispose(context);
                        }

                        DX11DynamicTexture2D t = new DX11DynamicTexture2D(context, this.FInW[0], this.FInH[0], SlimDX.DXGI.Format.B8G8R8A8_UNorm);


                        this.FTextureOutput[0][context] = t;
                    }
                    catch
                    {

                    }
                }

                if (this.copyframe && this.FTextureOutput[0].Contains(context))
                {
                    this.FTextureOutput[0][context].WriteData(this.rgbadata, this.size);
                }
            }
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            this.FTextureOutput[0].Dispose(context);
        }

        public void Dispose()
        {
            this.FTextureOutput[0].Dispose();
            if (c != null)
            {
                c.Close();
            }
        }
    }
}
