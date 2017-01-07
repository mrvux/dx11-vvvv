using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "CopyCounter", Category = "DX11.Buffer", Version = "", Author = "vux")]
    public class CopyCounterNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Buffer In", DefaultValue = 1,IsSingle=true)]
        protected Pin<DX11Resource<IDX11RWResource>> FInBuffer;

        [Output("Buffer Out",IsSingle=true)]
        protected ISpread<DX11Resource<DX11RawBuffer>> FOutBuffer;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutBuffer[0] == null)
            {
                this.FOutBuffer[0] = new DX11Resource<DX11RawBuffer>();
            }
        }

        public void Update(DX11RenderContext context)
        {
            Device device = context.Device;
            DeviceContext ctx = context.CurrentDeviceContext;

            if (!this.FOutBuffer[0].Contains(context))
            {
                DX11RawBuffer rb = new DX11RawBuffer(device, 16);
                this.FOutBuffer[0][context] = rb;
            }

            if (this.FInBuffer.IsConnected)
            {
                UnorderedAccessView uav = this.FInBuffer[0][context].UAV;
                ctx.CopyStructureCount(uav, this.FOutBuffer[0][context].Buffer, 0);
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutBuffer.SafeDisposeAll(context);
        }

        public void Dispose()
        {
            this.FOutBuffer.SafeDisposeAll();
        }
    }
}
