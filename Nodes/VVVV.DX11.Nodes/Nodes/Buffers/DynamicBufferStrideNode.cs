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
using SlimDX;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "DynamicBuffer", Category = "DX11.Buffer", Version = "Value Stride", Author = "vux")]
    public class DynamicFloatBufferStride : IPluginEvaluate, IDX11ResourceProvider, IPartImportsSatisfiedNotification, IDisposable
    {
        [Import()]
        protected IIOFactory iofactory;

        [Input("Custom Stride", DefaultValue = 4)]
        protected ISpread<int> FStride;

        [Config("Fixed Spread Count", DefaultValue = 0)]
        protected IDiffSpread<bool> FFixed;

        private IIOContainer<ISpread<int>> FCount;

        [Input("Data", DefaultValue = 0, AutoValidate = false, Order = 5)]
        protected ISpread<float> FInData;

        [Input("Keep In Memory", DefaultValue = 0, Order = 6)]
        protected ISpread<bool> FKeep;

        [Input("Apply", IsBang = true, DefaultValue = 1, Order = 7)]
        protected ISpread<bool> FApply;

        [Output("Buffer")]
        protected Pin<DX11Resource<DX11DynamicStructuredBuffer>> FOutput;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private bool FInvalidate;
        private bool FFirst = true;
        private int spreadmax;

        protected bool ffixed = false;

        protected float[] tempbuffer = new float[0];

        public void Evaluate(int SpreadMax)
        {
            this.spreadmax = SpreadMax;
            this.FOutput.SliceCount = SpreadMax > 0 ? 1 : 0;
            this.FValid.SliceCount = SpreadMax > 0 ? 1 : 0;
            this.FInvalidate = false;

            if (this.spreadmax > 0)
            {
                if (this.FOutput[0] == null) { this.FOutput[0] = new DX11Resource<DX11DynamicStructuredBuffer>(); }

                if (this.FApply[0] || this.FFirst)
                {

                    if (this.ffixed)
                    {
                        this.FCount.IOObject.Sync();
                    }

                    this.FInData.Sync();

                    this.FInvalidate = true;
                    this.FFirst = false;
                    this.FOutput.Stream.IsChanged = true;
                }
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (this.FInvalidate || !this.FOutput[0].Contains(context))
            {
                int count = this.ffixed ? this.FCount.IOObject[0] : this.FInData.SliceCount;
                count /= this.FStride[0];
                count *= 4;

                if (this.FOutput[0].Contains(context))
                {
                    if (this.FOutput[0][context].ElementCount != count)
                    {
                        this.FOutput[0].Dispose(context);
                    }
                }

                if (!this.FOutput[0].Contains(context))
                {
                    if (count > 0)
                    {
                        this.FOutput[0][context] = new DX11DynamicStructuredBuffer(context.Device, count,this.FStride[0]);
                        this.FValid[0] = true;
                    }
                    else
                    {
                        this.FValid[0] = false;
                    }
                }

                DX11DynamicStructuredBuffer b = this.FOutput[0][context];

                if (this.tempbuffer.Length != count)
                {
                    Array.Resize<float>(ref this.tempbuffer, count);
                }
                this.WriteArray(count);

                DataStream ds = b.MapForWrite(context.CurrentDeviceContext);
                ds.WriteRange<float>(this.tempbuffer);
                b.UnMap(context.CurrentDeviceContext);
            }

        }

        protected virtual void WriteArray(int count)
        {
            for (int i = 0; i < this.tempbuffer.Length; i++)
            {
                this.tempbuffer[i] = this.FInData[i];
            }

        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            if (force || !this.FKeep[0])
            {
                this.FOutput[0].Dispose(context);
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            try
            {
                this.FOutput[0].Dispose();
            }
            catch
            {

            }
        }
        #endregion

        public void OnImportsSatisfied()
        {
            this.FFixed.Changed += FFixed_Changed;
        }

        private void FFixed_Changed(IDiffSpread<bool> spread)
        {
            if (this.FFixed[0])
            {
                InputAttribute iattr = new InputAttribute("Element Count");
                iattr.AutoValidate = false;
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
