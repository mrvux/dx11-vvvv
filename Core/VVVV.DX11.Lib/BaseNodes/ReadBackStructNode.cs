using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

using SlimDX;

using FeralTic.DX11.Resources;
using FeralTic.DX11;
using SlimDX.Direct3D11;
using VVVV.Hosting.IO.Pointers;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace VVVV.DX11.Nodes
{
    public unsafe abstract class ReadBackBufferBaseNode<T> : IPluginEvaluate, IDX11ResourceDataRetriever where T: struct
    {
        [Input("Input", AutoValidate =false)]
        protected Pin<DX11Resource<IDX11StructuredBuffer>> FInput;

        [Input("Enabled", DefaultValue = 1, IsSingle = true)]
        protected ISpread<bool> FInEnabled;

        [Input("Double Buffer", Visibility =PinVisibility.OnlyInspector)]
        protected ISpread<bool> FInDoubleBuffer;

        [Output("Output")]
        protected ISpread<T> FOutput;

        [Import()]
        protected IPluginHost FHost;

        private DX11StagingStructuredBuffer stagingWrite;
        private DX11StagingStructuredBuffer stagingRead;

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;

        protected abstract void WriteData(DataStream ds, int elementcount);



        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            if (this.FInput.IsConnected && this.FInEnabled[0])
            {
                this.FInput.Sync();

                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null)
                {
                    this.FOutput.SliceCount = 0;
                    return;
                }

                IDX11StructuredBuffer b = this.FInput[0][this.AssignedContext];
                if (b != null)
                {
                    if (Marshal.SizeOf(typeof(T)) != b.Stride)
                    {
                        this.FOutput.SliceCount = 0;
                        this.FHost.Log(TLogType.Error, "Buffer has an invalid stride");
                        return;
                    }

                    if (this.stagingWrite != null && this.stagingWrite.ElementCount != b.ElementCount)
                    {
                        this.stagingWrite.Dispose(); this.stagingWrite = null;
                        this.stagingRead.Dispose(); this.stagingRead = null;
                    }

                    if (this.stagingWrite == null)
                    {
                        stagingWrite = new DX11StagingStructuredBuffer(this.AssignedContext.Device, b.ElementCount, b.Stride);
                        stagingRead = new DX11StagingStructuredBuffer(this.AssignedContext.Device, b.ElementCount, b.Stride);
                    }

                    this.AssignedContext.CurrentDeviceContext.CopyResource(b.Buffer, stagingWrite.Buffer);

                    this.FOutput.SliceCount = b.ElementCount;

                    DataStream ds = stagingRead.MapForRead(this.AssignedContext.CurrentDeviceContext);
                    try
                    {
                        
                        this.WriteData(ds, b.ElementCount);

                        this.FOutput.Flush(true);
                    }
                    catch (Exception ex)
                    {
                        FHost.Log(TLogType.Error, "Error inreadback node: " + ex.Message);
                    }
                    finally
                    {
                        stagingRead.UnMap(this.AssignedContext.CurrentDeviceContext);
                    }

                    SharpDX.Utilities.Swap(ref stagingWrite, ref stagingRead);

                }
                else
                {
                    this.FOutput.SliceCount = 0;
                }
            }
            else
            {
                this.FOutput.SliceCount = 0;
            }
        }
        #endregion

    }
}
