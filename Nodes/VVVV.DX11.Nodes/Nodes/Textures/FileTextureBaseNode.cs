using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.Resources;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System.IO;

namespace VVVV.DX11.Nodes
{
    public abstract class FileTextureLoadTask<T> : DX11AbstractLoadTask<T> where T : IDX11Resource
    {
        public int Slice { get; protected set; }
        protected string path;

        public FileTextureLoadTask(DX11RenderContext context, int slice, string path)
            : base(context)
        {
            this.Slice = slice;
            this.path = path;
        }

        public override void Dispose()
        {
            if (this.Resource != null) { this.Resource.Dispose(); }
        }
    }

    public abstract class FileTextureBaseNode<T> : IPluginEvaluate, IDX11ResourceHost, IDisposable where T : IDX11Resource
    {
        [Input("Filename", StringType = StringType.Filename)]
        protected IDiffSpread<string> FInPath;

        [Input("Load In Background",IsSingle=true)]
        protected ISpread<bool> FInBGLoad;

        [Input("Reload", IsBang = true)]
        protected ISpread<bool> FInReload;

        [Input("Keep In Memory", Visibility = PinVisibility.Hidden)]
        protected ISpread<bool> FInKeep;

        [Input("No Mips", Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<bool> FInNoMips;

        [Output("Texture Out", Order=1)]
        protected ISpread<DX11Resource<T>> FTextureOutput;

        [Output("Is Valid", Order = 500)]
        protected ISpread<bool> FValid;

        //protected static Dictionary<string, T> respool = new Dictionary<string, T>();

        private bool FInvalidate;
        private bool FDestroyed;

        protected abstract bool Load(DX11RenderContext context, string path, out T result);

        protected abstract FileTextureLoadTask<T> GetTask(DX11RenderContext context, string path, int slice);

        private List<FileTextureLoadTask<T>> tasks = new List<FileTextureLoadTask<T>>();

        private object m_lock = new object();

        public void Evaluate(int SpreadMax)
        {
            if (this.FInPath.IsChanged || this.FInReload[0] || this.FInNoMips.IsChanged)
            {

                this.SliceCountchanged(SpreadMax);

                this.FTextureOutput.SafeDisposeAll();

                foreach (FileTextureLoadTask<T> task in this.tasks)
                {
                    task.MarkForAbort();
                }

                for (int i = 0; i < SpreadMax; i++ )
                {
                    if (File.Exists(this.FInPath[i]))
                    {
                        this.LoadInfo(i, this.FInPath[i]);
                    }
                    else
                    {
                        this.MarkInvalid(i);
                    }
                    
                }


                this.FTextureOutput.SliceCount = SpreadMax;
                this.FValid.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FTextureOutput[i] = new DX11Resource<T>();
                    this.FValid[i] = false;
                }

                this.FInvalidate = true;
            }     
        }

        protected virtual void SliceCountchanged(int slicecount) { }
        protected abstract void LoadInfo(int slice, string path);
        protected abstract void MarkInvalid(int slice);

        public void Update(DX11RenderContext context)
        {
            if (this.FTextureOutput.SliceCount == 0)
                return;

            if (this.FInvalidate || this.FTextureOutput[0].Contains(context) == false || this.FDestroyed)
            {
                for (int i = 0; i < this.FInPath.SliceCount; i++)
                {
                    T result;

                    if (File.Exists(this.FInPath[i]))
                    {
                        if (this.FInBGLoad[0])
                        {

                            FileTextureLoadTask<T> task = this.GetTask(context, this.FInPath[i], i);
                            task.StatusChanged += new TaskStatusChangedDelegate(task_StatusChanged);
                            this.tasks.Add(task);
                        }
                        else
                        {

                            bool res = this.Load(context, this.FInPath[i], out result);
                            this.FTextureOutput[i][context] = result;
                            this.FValid[i] = res;
                        }
                    }
                    else
                    {
                        this.FTextureOutput[i][context] = default(T);
                        this.FValid[i] = false;
                    }
                }

                this.FInvalidate = false;
                this.FDestroyed = false;
            }

            
        }

        void task_StatusChanged(IDX11ScheduledTask task)
        {
            /*if (task.Status == eDX11SheduleTaskStatus.Completed
                || task.Status == eDX11SheduleTaskStatus.Aborted
                || task.Status == eDX11SheduleTaskStatus.Error)
            {
                this.tasks.Remove(task);
            }

            if (task.Status == eDX11SheduleTaskStatus.Completed)
            {
                this.FTextureOutput[task.
            }*/
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (force || !this.FInKeep[0])
            {
                for (int i = 0; i < this.FTextureOutput.SliceCount; i++)
                {
                    this.FTextureOutput[i].Dispose(context);
                }

                foreach (FileTextureLoadTask<T> task in this.tasks)
                {
                    task.MarkForAbort();
                }
                this.tasks.Clear();

                this.FDestroyed = true;
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            this.FTextureOutput.SafeDisposeAll();
        }
        #endregion
    }
}
