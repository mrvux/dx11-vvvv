using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;


using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using VVVV.DX11;

using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    public class DynamicStructBuffer<T> : IPluginEvaluate, IDX11ResourceHost,IPartImportsSatisfiedNotification, IDisposable where T : struct
    {
        [Import()]
        protected IIOFactory iofactory;

        [Config("Fixed Spread Count", DefaultValue = 0)]
        protected IDiffSpread<bool> FFixed;

        private IIOContainer<ISpread<int>> FCount;

        [Input("Data", DefaultValue = 0, AutoValidate = false,Order=5)]
        protected ISpread<T> FInData;

        [Input("Keep In Memory", DefaultValue = 0,Order=6)]
        protected ISpread<bool> FKeep;

        [Input("Preferred Buffer Type", DefaultValue = 0, Order = 6, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<DX11BufferUploadType> FBufferType;

        [Input("Apply", IsBang = true, DefaultValue = 1,Order=7)]
        protected ISpread<bool> FApply;

        [Output("Buffer")]
        protected ISpread<DX11Resource<IDX11ReadableStructureBuffer>> FOutput;

        DX11BufferUploadType bufferType = DX11BufferUploadType.Dynamic;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private bool FInvalidate;
        private bool FFirst = true;
        private int spreadmax;

        protected bool ffixed = false;
        protected virtual bool NeedConvert { get { return false; } }

        protected T[] tempbuffer = new T[0];

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (this.FApply[0] || this.FFirst)
            {

                if (this.ffixed)
                {
                    this.FCount.IOObject.Sync();
                }

                this.FInData.Sync();

                if (this.FInData.SliceCount > 0)
                {
                    this.FOutput.SliceCount = 1;
                    this.FValid.SliceCount = 1;
                    if (this.FOutput[0] == null) { this.FOutput[0] = new DX11Resource<IDX11ReadableStructureBuffer>(); }
                }
                else
                {
                    if (this.FOutput.SliceCount > 0 && this.FOutput[0] != null)
                    {
                        this.FOutput[0].Dispose();
                    }
                    this.FOutput.SliceCount = 0;
                    this.FValid.SliceCount = 0;
                }

                this.spreadmax = this.FInData.SliceCount;

                this.FInvalidate = true;
                this.FFirst = false;
                this.FOutput.Stream.IsChanged = true;
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.spreadmax == 0) { return; }

            if (this.FInvalidate || !this.FOutput[0].Contains(context))
            {
                int count = this.ffixed ? this.FCount.IOObject[0] : this.FInData.SliceCount;

                if (this.FOutput[0].Contains(context))
                {
                    if (this.FOutput[0][context].ElementCount != count 
                        || this.bufferType != this.FBufferType[0] 
                        || this.FOutput[0][context] is DX11ImmutableStructuredBuffer<T>)
                    {
                        this.FOutput[0].Dispose(context);
                    }
                }

                if (this.tempbuffer.Length != count)
                {
                    Array.Resize<T>(ref this.tempbuffer, count);
                }

                //If fixed or if size is the same, we can do a direct copy
                bool needconvert = ((this.ffixed && count != this.FInData.SliceCount)) || this.NeedConvert;

                T[] bufferToCopy = this.FInData.Stream.Buffer;
                int bufferElementCount = this.FInData.SliceCount;

                if (needconvert)
                {
                    this.WriteArray(count);
                    bufferToCopy = this.tempbuffer;
                    bufferElementCount = this.tempbuffer.Length;
                }

               
                if (!this.FOutput[0].Contains(context))
                {
                    if (count > 0)
                    {
                        if (this.FBufferType[0] == DX11BufferUploadType.Dynamic)
                        {
                            this.FOutput[0][context] = new DX11DynamicStructuredBuffer<T>(context, count);
                        }
                        else if (this.FBufferType[0] == DX11BufferUploadType.Default)
                        {
                            this.FOutput[0][context] = new DX11CopyDestStructuredBuffer<T>(context, count);
                        }
                        else
                        {
                            this.FOutput[0][context] = new DX11ImmutableStructuredBuffer<T>(context.Device, bufferToCopy, bufferToCopy.Length);
                        }

                        this.FValid[0] = true;
                        this.bufferType = this.FBufferType[0];
                    }
                    else
                    {
                        this.FValid[0] = false;
                        return;
                    }
                }

                bool needContextCopy = this.FBufferType[0] != DX11BufferUploadType.Immutable;
                if (needContextCopy)
                {
                    try
                    {
                        if (this.FBufferType[0] == DX11BufferUploadType.Dynamic)
                        {
                            DX11DynamicStructuredBuffer<T> b = (DX11DynamicStructuredBuffer<T>)this.FOutput[0][context];
                            b.WriteData(bufferToCopy, 0, b.ElementCount);
                        }
                        else if (this.FBufferType[0] == DX11BufferUploadType.Default)
                        {
                            DX11CopyDestStructuredBuffer<T> b = (DX11CopyDestStructuredBuffer<T>)this.FOutput[0][context];
                            b.WriteData(bufferToCopy, 0, b.ElementCount);
                        }

                    }
                    catch (Exception ex)
                    {
                        this.iofactory.PluginHost.Log(TLogType.Error, ex.Message);
                    }
                }
            }

        }

        protected virtual void WriteArray(int count)
        {
            for (int i = 0; i < this.tempbuffer.Length; i++)
            {
                this.tempbuffer[i] = this.FInData[i];
            }

        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (force || !this.FKeep[0])
            {
                this.FOutput.SafeDisposeAll(context);
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            this.FOutput.SafeDisposeAll();
        }
        #endregion

        public void OnImportsSatisfied()
        {
            this.FOutput.SliceCount = 1;

            this.FFixed.Changed += FFixed_Changed;
        }

        private void FFixed_Changed(IDiffSpread<bool> spread)
        {
            if (this.FFixed[0])
            {
                InputAttribute iattr = new InputAttribute("Element Count");
                iattr.AutoValidate=false;
                iattr.DefaultValue = 1;
                this.FCount = iofactory.CreateIOContainer<ISpread<int>>(iattr);

                this.ffixed = true;
            }
            else
            {
                if (this.FCount != null)
                {
                    this.FCount.Dispose();
                    this.FCount = null;
                }

                this.ffixed = false;
            }
        }
    }

    
}
