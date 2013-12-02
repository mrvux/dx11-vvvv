using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;
using SlimDX;

namespace FeralTic.DX11.Resources
{
    public class DXWriteableRawBuffer:  IDX11RWResource
    {
        public UnorderedAccessView UAV { get; protected set; }

        public ShaderResourceView SRV { get; protected set; }

        public Buffer Buffer { get; protected set; }

        public int Size { get; protected set; }

        public DXWriteableRawBuffer(DX11RenderContext context, Buffer buffer)
        {
            this.Size = buffer.Description.SizeInBytes;
            this.Buffer = buffer;

            ShaderResourceViewDescription srvd = new ShaderResourceViewDescription()
            {
                Format = SlimDX.DXGI.Format.R32_Typeless,
                Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                Flags = ShaderResourceViewExtendedBufferFlags.RawData,
                ElementCount = this.Size / 4
            };
            this.SRV = new ShaderResourceView(context.Device, this.Buffer, srvd);
        }

        public DXWriteableRawBuffer(Device dev, int size)
        {
            this.Size = size;

            BufferDescription bd = new BufferDescription()
            {
                BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.RawBuffer,
                SizeInBytes = this.Size,
                Usage = ResourceUsage.Default,
            };
            this.Buffer = new Buffer(dev, bd);

            ShaderResourceViewDescription srvd = new ShaderResourceViewDescription()
            {
                Format = SlimDX.DXGI.Format.R32_Typeless,
                Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                Flags = ShaderResourceViewExtendedBufferFlags.RawData,
                ElementCount = size / 4
            };
            this.SRV = new ShaderResourceView(dev, this.Buffer, srvd);

            UnorderedAccessViewDescription uavd = new UnorderedAccessViewDescription()
            {
                Format = SlimDX.DXGI.Format.R32_Typeless,
                Dimension = UnorderedAccessViewDimension.Buffer,
                Flags = UnorderedAccessViewBufferFlags.RawData,
                ElementCount = size / 4
            };

            this.UAV = new UnorderedAccessView(dev, this.Buffer, uavd);
        }

        public void Dispose()
        {
            if (this.SRV != null) { this.SRV.Dispose(); }
            if (this.UAV != null) { this.UAV.Dispose(); }
            if (this.Buffer != null) { this.Buffer.Dispose(); }
        }
    }
}
