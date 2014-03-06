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
    public class DynamicStructBufferBin<T> : IPluginEvaluate, IDX11ResourceProvider, IDisposable where T : struct
    {

        [Input("Data")]
        protected ISpread<ISpread<T>> FInData;

        [Input("Keep In Memory", DefaultValue = 0,Order=6)]
        protected ISpread<bool> FKeep;

        [Input("Apply", IsBang = true, DefaultValue = 1,Order=7)]
        protected ISpread<bool> FApply;

        [Output("Buffer")]
        protected Pin<DX11Resource<DX11DynamicStructuredBuffer<T>>> FOutput;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private bool FInvalidate;
        private bool FFirst = true;
        private int spreadmax;

        protected bool ffixed = false;

        protected T[] tempbuffer = new T[0];

        public void Evaluate(int SpreadMax)
        {
            this.spreadmax = FInData.SliceCount;
            this.FOutput.SliceCount = FInData.SliceCount;
            this.FValid.SliceCount = FInData.SliceCount;
            this.FInvalidate = false;

            if (this.spreadmax > 0)
            {
                for (int i = 0; i < this.spreadmax; i++)
                {
                    if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<DX11DynamicStructuredBuffer<T>>(); }

                    if (this.FApply[i] || this.FFirst)
                    {
                        this.FInData.Sync();

                        this.FInvalidate = true;
                        this.FFirst = false;
                        this.FOutput.Stream.IsChanged = true;
                    }
                }
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            for (int i = 0; i < this.spreadmax; i++ )
                if (this.FInvalidate || !this.FOutput[i].Contains(context))
                {
                    int count = this.FInData[i].SliceCount;

                    if (this.FOutput[i].Contains(context))
                    {
                        if (this.FOutput[i][context].ElementCount != count)
                        {
                            this.FOutput[i].Dispose(context);
                        }
                    }

                    if (!this.FOutput[i].Contains(context))
                    {
                        if (count > 0)
                        {
                            this.FOutput[i][context] = new DX11DynamicStructuredBuffer<T>(context, count);
                            this.FValid[i] = true;
                        }
                        else
                        {
                            this.FValid[i] = false;
                            return;
                        }
                    }

                    DX11DynamicStructuredBuffer<T> b = this.FOutput[i][context];

                    if (this.tempbuffer.Length != count)
                    {
                        Array.Resize<T>(ref this.tempbuffer, count);
                    }
                    this.WriteArray(count, i);

                    b.WriteData(this.tempbuffer);
                }

        }

        protected virtual void WriteArray(int count, int i)
        {
            for (int j = 0; j < this.tempbuffer.Length; j++)
            {
                this.tempbuffer[j] = this.FInData[i][j];
            }

        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            if (force || !this.FKeep[0])
            {
                for (int i = 0; i < this.spreadmax; i++)
                    this.FOutput[i].Dispose(context);
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            try
            {
                for (int i = 0; i < this.spreadmax; i++)
                    this.FOutput[i].Dispose();
            }
            catch
            {

            }
        }
        #endregion
    }

    
}
