using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using VVVV.MSKinect.Lib;
using System.Runtime.InteropServices;

namespace VVVV.DX11.Nodes.MSKinect
{
    public abstract class KinectBaseTextureNode : IPluginEvaluate, IPluginConnections, IDX11ResourceHost, IDisposable
    {
         [DllImport("msvcrt.dll", SetLastError = false)]
        protected static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

        [Input("Kinect Runtime", Order = -20)]
        public Pin<KinectRuntime> FInRuntime;

        [Output("Texture", IsSingle = true)]
        public Pin<DX11Resource<DX11DynamicTexture2D>> FTextureOutput;

        [Output("Frame Index", IsSingle = true, Order = 10)]
        public ISpread<long> FOutFrameIndex;

        protected long frameindex = -1;

        private bool FInvalidateConnect = false;
        protected bool FInvalidate = true;

        protected bool Resized = false;

        protected KinectRuntime runtime;

        protected object m_lock = new object();

        protected abstract int Width { get; }
        protected abstract int Height { get; }
        protected abstract SlimDX.DXGI.Format Format { get; }
        protected abstract void CopyData(DX11DynamicTexture2D texture);
        protected abstract void OnRuntimeConnected();
        protected abstract void OnRuntimeDisconnected();
        protected virtual void Disposing() { }

        protected virtual void OnEvaluate() { }

        public void Evaluate(int SpreadMax)
        {
            if (this.FTextureOutput[0] == null) { this.FTextureOutput[0] = new DX11Resource<DX11DynamicTexture2D>(); }

            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.OnRuntimeDisconnected();
                }

                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];
                    this.OnRuntimeConnected();
                }
                else
                {
                    this.OnRuntimeDisconnected();
                }

                this.FInvalidateConnect = false;
            }

            this.OnEvaluate();

            this.FOutFrameIndex[0] = this.frameindex;
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

        protected void DisposeTextures()
        {
            this.FTextureOutput[0].Dispose();
        }

        public void Update(DX11RenderContext context)
        {
            if (!this.FTextureOutput[0].Contains(context))
            {
                this.FTextureOutput[0][context] = new DX11DynamicTexture2D(context, this.Width, this.Height, this.Format);
            }

            if (this.Resized)
            {
                this.FTextureOutput[0].Dispose(context);
                this.FTextureOutput[0][context] = new DX11DynamicTexture2D(context, this.Width, this.Height, this.Format);
                this.Resized = false;
            }


            if (this.FInvalidate)
            {
                lock (m_lock)
                {
                    this.CopyData(this.FTextureOutput[0][context]);
                }
                this.FInvalidate = false;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FTextureOutput[0].Dispose(context);
        }

        public void Dispose()
        {
            if (this.runtime != null)
            {
                //Force a disconnect, to unregister event
                this.OnRuntimeDisconnected();
            }

            this.Disposing();

            if (this.FTextureOutput[0] != null)
            {
                this.FTextureOutput[0].Dispose();
            }
        }
    }
}
