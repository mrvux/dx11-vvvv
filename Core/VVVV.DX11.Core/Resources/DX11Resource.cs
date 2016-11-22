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
    public class DX11Resource<T> : IDX11ResourceDataProvider, IDX11ResourceDataSink where T : IDX11Resource
    {
        private Dictionary<DX11RenderContext, IDX11Resource> resources = new Dictionary<DX11RenderContext, IDX11Resource>();

        private object syncRoot = new object();

        public DX11Resource()
        {
            this.resources = new Dictionary<DX11RenderContext, IDX11Resource>();

        }

        public void Assign(IDX11ResourceDataProvider original)
        {
            if (original != null)
            {
                this.resources = original.Data;
            }
            else
            {
                this.resources = null;
            }
        }

        public Dictionary<DX11RenderContext, IDX11Resource> Data
        {
            get { return this.resources; }
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
        /// Dispose internal resource for all devices
        /// </summary>
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
                    //this.resources[device].Dispose();
                    if (resources[context] is IDisposable)
                    {
                        IDisposable d = resources[context] as IDisposable;
                        d.Dispose();
                    }
                    this.resources.Remove(context);
                }
            }
        }

        public void Remove(DX11RenderContext context)
        {
            lock (syncRoot)
            {
                if (this.resources.ContainsKey(context))
                {
                    this.resources.Remove(context);
                }
            }
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
