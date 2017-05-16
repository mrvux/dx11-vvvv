using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using SlimDX;
using FeralTic.DX11.Resources;
using FeralTic.DX11;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "DynamicBuffer", Category = "DX11.Buffer", Version = "Raw", Author = "vux")]
    public class DynamicRawBuffer : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Input", DefaultValue = 1, AutoValidate = false)]
        protected ISpread<System.IO.Stream> streamInput;

        [Input("Apply", IsBang = true, DefaultValue = 1, Order = 100)]
        protected ISpread<bool> FApply;

        [Output("Buffer", IsSingle = true)]
        protected ISpread<DX11Resource<DX11DynamicRawBuffer>> FOutput;

        private bool FInvalidate;
        private bool FFirst = true;

        private SlimDX.DataStream dataStream;

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = 1;
            this.FInvalidate = false;

            if (this.FOutput[0] == null) { this.FOutput[0] = new DX11Resource<DX11DynamicRawBuffer>(); }

            if (this.FApply[0] || this.FFirst)
            {
                this.streamInput.Sync();

                if (this.streamInput[0] != null)
                {
                    var inStream = this.streamInput[0];

                    if (this.dataStream != null && this.dataStream.Length  != inStream.Length)
                    {
                        this.dataStream.Dispose();
                        this.dataStream = null;
                    }

                    if (this.dataStream == null && inStream.Length > 0)
                    {
                        this.dataStream = new DataStream(inStream.Length, true, true);
                    }


                    if (this.dataStream != null)
                    {

                        inStream.Position = 0;
                        dataStream.Position = 0;

                        inStream.CopyTo(dataStream);
                        dataStream.Position = 0;
                    }

                    
                }
                else
                {
                    if (this.dataStream != null)
                    {
                        this.dataStream.Dispose();
                        this.dataStream = null;
                    }
                }

                this.FInvalidate = true;
                this.FFirst = false;
                this.FOutput.Stream.IsChanged = true;
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInvalidate && this.dataStream != null)
            {
                Device device = context.Device;

                if (this.FOutput[0].Contains(context))
                {
                    if (this.FOutput[0][context].Size != this.dataStream.Length) 
                    {
                        this.FOutput[0].Dispose(context);
                        this.FOutput[0][context] = new DX11DynamicRawBuffer(context, (int)this.dataStream.Length);
                    }
                }
                else
                {
                    this.FOutput[0][context] = new DX11DynamicRawBuffer(context, (int)this.dataStream.Length);
                }

                this.FOutput[0][context].WriteData(this.dataStream.DataPointer, (int)this.dataStream.Length);
                this.FInvalidate = false;
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
