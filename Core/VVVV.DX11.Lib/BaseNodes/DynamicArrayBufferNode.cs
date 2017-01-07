using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using SlimDX;
using VVVV.DX11;
using VVVV.DX11.Lib.Devices;

using FeralTic.DX11.Resources;
using FeralTic.DX11;


namespace VVVV.DX11.Nodes
{

    public abstract class DynamicArrayBuffer<T> : IPluginEvaluate, IDX11ResourceHost, IDisposable where T : struct
    {
        [Input("Element Count", DefaultValue = 1, AutoValidate = false)]
        protected ISpread<int> FInCount;

        [Input("Apply", IsBang = true, DefaultValue = 1, Order = 100)]
        protected ISpread<bool> FApply;

        [Output("Buffer",IsSingle=true)]
        protected ISpread<DX11Resource<DX11DynamicStructuredBuffer<T>>> FOutput;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private bool FInvalidate;
        private bool FFirst = true;

        private T[] m_data;

        protected abstract void BuildBuffer(int count, T[] buffer);

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = 1;
            this.FValid.SliceCount = SpreadMax;
            this.FInvalidate = false;

            if (this.FOutput[0] == null) { this.FOutput[0] = new DX11Resource<DX11DynamicStructuredBuffer<T>>(); }

            if (this.FApply[0] || this.FFirst)
            {
                this.FInCount.Sync();

                if (this.m_data == null) { this.m_data = new T[this.FInCount.SliceCount]; }

                if (this.m_data.Length != this.FInCount[0])
                {
                    this.m_data = new T[this.FInCount[0]];
                }

                this.BuildBuffer(this.FInCount[0], this.m_data);

                this.FInvalidate = true;
                this.FFirst = false;
                this.FOutput.Stream.IsChanged = true;
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInvalidate)
            {
                Device device = context.Device;

                if (this.FOutput[0].Contains(context))
                {
                    if (this.FOutput[0][context].ElementCount != FInCount[0])
                    {
                        if (this.FInCount[0] > 0)
                        {
                            this.FOutput[0].Dispose(context);
                            this.FOutput[0][context] = new DX11DynamicStructuredBuffer<T>(context, FInCount[0]);
                        }
                    }
                }
                else
                {
                    if (this.FInCount[0] > 0)
                    {
                        this.FOutput[0][context] = new DX11DynamicStructuredBuffer<T>(context, FInCount[0]);
                    }
                }

                if (this.FInCount[0] > 0)
                {
                    DX11DynamicStructuredBuffer<T> b = this.FOutput[0][context];

                    b.WriteData(this.m_data);
                }
            }

        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (force)
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
    }
}
