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
using System.IO;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "BufferData", Category = "DX11.RawBuffer", Version = "", Author = "vux", Credits = "vux", Help ="Copies a raw buffer data content into a cpu stream")]
    public class RawBufferStreamDataNode : IPluginEvaluate, IDX11ResourceDataRetriever, IPartImportsSatisfiedNotification
    {
        [Input("Buffer In", IsSingle = true, AutoValidate = false)]
        protected Pin<DX11Resource<IDX11Buffer>> FBufferIn;

        [Input("Enabled", DefaultValue = 1, IsSingle = true)]
        protected ISpread<bool> FRead;

        [Output("Output")]
        public ISpread<Stream> FStreamOut;

        [Output("Output Stride")]
        public ISpread<int> strideOut;

        [Output("Valid")]
        protected ISpread<bool> FOutValid;

        [Import()]
        protected IPluginHost FHost;

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;

        //called when all inputs and outputs defined above are assigned from the host
        public void OnImportsSatisfied()
        {
            //start with an empty stream output
            FStreamOut.SliceCount = 0;
        }

        MemoryStream lastStream;
        DX11StagingRawBuffer lastBuffer;

        #region IPluginEvaluate Members

        public void Evaluate(int SpreadMax)
        {
            this.FOutValid.SliceCount = 1;
            this.FStreamOut.SliceCount = 1;

            if (this.FRead[0])
            {
                this.FBufferIn.Sync();
                if (this.FBufferIn.IsConnected)
                {
                    if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                    if (this.AssignedContext == null) { this.FOutValid.SliceCount = 0; return; }

                    var context = this.AssignedContext;

                    if (this.FBufferIn[0].Contains(this.AssignedContext))
                    {
                        var rawBuffer = this.FBufferIn[0][context];

                        if (this.lastBuffer != null)
                        {
                            if (rawBuffer.Buffer.Description.SizeInBytes != lastBuffer.Size)
                            {
                                this.lastStream.Dispose();
                                this.lastStream = null;
                                this.lastBuffer.Dispose();
                                this.lastBuffer = null;
                            }
                        }

                        if (this.lastBuffer == null)
                        {
                            this.lastBuffer = new DX11StagingRawBuffer(context.Device, rawBuffer.Buffer.Description.SizeInBytes);
                            this.lastStream = new MemoryStream(rawBuffer.Buffer.Description.SizeInBytes);
                        }
                        lastStream.Position = 0;
                        context.CurrentDeviceContext.CopyResource(rawBuffer.Buffer, this.lastBuffer.Buffer);

                        DataStream ds = this.lastBuffer.MapForRead(context.CurrentDeviceContext);
                        ds.CopyTo(lastStream);
                        lastStream.Position = 0;
                        this.lastBuffer.UnMap(context.CurrentDeviceContext);

                        FStreamOut[0] = lastStream;
                        FStreamOut.Flush(true);
                    }
                }
            }
        }

        #endregion
    }
}
