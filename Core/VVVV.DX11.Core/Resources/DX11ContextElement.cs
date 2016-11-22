using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D11;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11
{
    /// <summary>
    /// Main resource holder for per data device, to be used by any pin which holds a dx11 resource
    /// </summary>
    /// <typeparam name="T">Resource Type</typeparam>
    public class DX11ContextElement<T> : IDisposable
    {
        private Dictionary<DX11RenderContext, T> resources = new Dictionary<DX11RenderContext, T>();

        private object syncRoot = new object();

        public DX11ContextElement()
        {
            this.resources = new Dictionary<DX11RenderContext, T>();

        }

        public bool Contains(DX11RenderContext context)
        {
            lock (syncRoot)
            {
                if (this.resources == null) { return false; }
                return this.resources.ContainsKey(context);
            }
        }

         /// <summary>
        /// Dispose resource for a single device
        /// </summary>
        /// <param name="device">Device to dispose resource</param>
        public void Dispose(DX11RenderContext context)
        {
            lock (syncRoot)
            {
                if (this.resources.ContainsKey(context))
                {
                    if (resources[context] is IDisposable)
                    {
                        IDisposable d = resources[context] as IDisposable;
                        d.Dispose();
                    }
                    this.resources.Remove(context);
                }
            }
        }

        public void Dispose()
        {
            lock (syncRoot)
            {
                //Dispose resource for all devices
                foreach (DX11RenderContext context in this.resources.Keys)
                {
                    if (resources[context] is IDisposable)
                    {
                        IDisposable d = resources[context] as IDisposable;
                        d.Dispose();
                    }
                    //resources[dev].Dispose();
                }
                resources.Clear();
            }
        }

        public void Clear()
        {
            this.resources.Clear();
        }

        /// <summary>
        /// Assigns or retrieve resource from a device
        /// </summary>
        /// <param name="device">Device</param>
        /// <returns>Resource for this device</returns>
        public T this[DX11RenderContext context]
        {
            get
            {
                lock (syncRoot)
                {
                    if (this.resources.ContainsKey(context))
                    {
                        return (T)this.resources[context];
                    }
                    else
                    {
                        return default(T);
                    }
                }
            }
            set
            {
                lock (syncRoot)
                {
                    this.resources[context] = value;
                }
            }
        }

    }
}
