using FeralTic.DX11;
using FeralTic.DX11.Resources;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11.Lib
{
    public delegate void FileTextureLoadedDelegate(DX11FileTexturePoolElement element);

    public class DX11FileTexturePoolElement
    {
        public event FileTextureLoadedDelegate OnLoadComplete;

        private FileTexture2dLoadTask m_task;


        public DX11FileTexturePoolElement(DX11RenderContext context, string path, bool mips, bool async = false)
        {
            this.FileName = path;
            this.DoMips = mips;
            this.refcount = 1;


            if (async)
            {
                this.m_task = new FileTexture2dLoadTask(context, path, mips);
                m_task.StatusChanged += task_StatusChanged;

                context.ResourceScheduler.AddTask(m_task);
            }
            else
            {
                try
                {

                    ImageLoadInformation info = ImageLoadInformation.FromDefaults();
                    if (!this.DoMips)
                    {
                        info.MipLevels = 1;
                    }

                    this.Texture = DX11Texture2D.FromFile(context, path, info);
                    this.Status = eDX11SheduleTaskStatus.Completed;
                }
                catch
                {
                    this.Status = eDX11SheduleTaskStatus.Error;
                }
            }
        }

        private void task_StatusChanged(IDX11ScheduledTask task)
        {
            this.Status = task.Status;
            if (this.Status == eDX11SheduleTaskStatus.Completed || this.Status == eDX11SheduleTaskStatus.Error)
            {
                this.Texture = m_task.Resource;

                if (this.OnLoadComplete != null)
                {
                    this.OnLoadComplete(this);
                }
            }
        }

        public eDX11SheduleTaskStatus Status { get; private set; }

        public DX11Texture2D Texture { get; private set; }
        public string FileName { get; private set; }
        public bool DoMips { get; private set; }

        private int refcount = 0;

        public void IncrementCounter()
        {
            this.refcount++;
        }

        public void DecrementCounter()
        {
            this.refcount--;
        }

        public void MarkForAbort()
        {
            this.m_task.MarkForAbort();
        }

        public int RefCount
        {
            get { return this.refcount; }
        }
    }

    public class DX11FileTexturePool
    {
        private object m_lock = new object();


        private List<DX11FileTexturePoolElement> pool = new List<DX11FileTexturePoolElement>();

        public event EventHandler OnElementLoaded;

        public bool TryGetFile(DX11RenderContext context, string path, bool domips,bool async, out DX11Texture2D texture)
        {
            lock (m_lock)
            {
                foreach (DX11FileTexturePoolElement element in this.pool)
                {
                    if (element.DoMips == domips && element.FileName == path)
                    {
                        if (element.Status == eDX11SheduleTaskStatus.Completed)
                        {
                            element.IncrementCounter();
                            texture = element.Texture;
                            return true;
                        }
                        else
                        {
                            element.IncrementCounter();
                            texture = null;
                            return false;
                        }
                    }
                }
            }


            DX11FileTexturePoolElement elem = new DX11FileTexturePoolElement(context, path, domips,async);
            elem.OnLoadComplete += elem_OnLoadComplete;
            
            lock (m_lock)
            {
                if (elem.Status != eDX11SheduleTaskStatus.Error)
                {
                    this.pool.Add(elem);
                    if (elem.Status == eDX11SheduleTaskStatus.Completed)
                    {
                        texture = elem.Texture;
                        return true;
                    }
                    else
                    {
                        texture = null;
                        return false;
                    }

                }
                else
                {
                    texture = null;
                    return false;
                }
            }

        }

        void elem_OnLoadComplete(DX11FileTexturePoolElement element)
        {
            if (this.OnElementLoaded != null)
            {
                this.OnElementLoaded(this, new EventArgs());
            }
        }

        public void DecrementAll()
        {
            lock (m_lock)
            {
                foreach (DX11FileTexturePoolElement e in this.pool)
                {
                    e.DecrementCounter();
                }
            }
        }

        public void Flush()
        {
            lock (m_lock)
            {
                List<DX11FileTexturePoolElement> newlist = new List<DX11FileTexturePoolElement>();
                foreach (DX11FileTexturePoolElement e in this.pool)
                {
                    if (e.RefCount < 0 || (e.Status == eDX11SheduleTaskStatus.Error || e.Status == eDX11SheduleTaskStatus.Aborted))
                    {
                        if (e.Status == eDX11SheduleTaskStatus.Queued)
                        {
                            e.MarkForAbort();
                        }

                        if (e.Texture != null) { e.Texture.Dispose(); }
                    }
                    else
                    {
                        newlist.Add(e);
                    }
                }
                this.pool = newlist;
            }
        }

        public void Dispose()
        {
            foreach (DX11FileTexturePoolElement elem in this.pool)
            {
                elem.Texture.Dispose();
            }
            this.pool.Clear();
        }
    }
}
