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

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "ReadBackCounter", Category = "DX11.Buffer", Version = "", Author = "vux", Help ="Read back append or counter from uav, directly back into cpu")]
    public class ReadBackCounterBuffer : IPluginEvaluate, IDX11ResourceDataRetriever
    {
        [Input("Input", AutoValidate =false)]
        protected Pin<DX11Resource<IDX11RWStructureBuffer>> FInput;

        [Input("Enabled", DefaultValue = 1, IsSingle =true)]
        protected ISpread<bool> doRead;

        [Output("Output")]
        protected ISpread<int> output;

        [Import()]
        protected IPluginHost FHost;

        private DX11Resource<DX11StagingRawBuffer> copyBuffer = new DX11Resource<DX11StagingRawBuffer>();

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;


        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            if (this.doRead.SliceCount == 0)
                return;

            if (this.FInput.IsConnected && this.doRead[0])
            {
                this.FInput.Sync();

                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                IDX11RWStructureBuffer b = this.FInput[0][this.AssignedContext];

                if (b != null)
                {
                    if (!this.copyBuffer.Contains(this.AssignedContext))
                    {
                        this.copyBuffer[this.AssignedContext] = new DX11StagingRawBuffer(this.AssignedContext.Device, 16);
                    }

                    this.AssignedContext.CurrentDeviceContext.CopyStructureCount(b.UAV, this.copyBuffer[this.AssignedContext].Buffer, 0);

                    DataStream ds = this.copyBuffer[this.AssignedContext].MapForRead(this.AssignedContext.CurrentDeviceContext);
                    this.output.SliceCount = 1;
                    this.output[0] = ds.Read<int>();


                    this.copyBuffer[this.AssignedContext].UnMap(this.AssignedContext.CurrentDeviceContext);

                }
                else
                {
                    this.output.SliceCount = 0;
                }
            }
            else
            {
                this.output.SliceCount = 0;
            }
        }
        #endregion
    }
}
