using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D11;

using Buffer = SlimDX.Direct3D11.Buffer;

namespace VVVV.DX11.Nodes.Textures
{
    [PluginInfo(Name = "FromSharedResource", Category = "DX11.Buffer", Version = "Structured", Author = "vux", Help ="Gets a shared structured buffer from a shared handle, this is equivalent to shared texture but for buffers")]
    public class FromSharedStructuredBufferNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        private class DX11SharedStructuredBuffer : DX11StructuredBuffer, IDX11ReadableStructureBuffer
        {
            public ShaderResourceView SRV { get; private set; }

            public DX11SharedStructuredBuffer(DX11RenderContext context, IntPtr sharedHandle)
            {
                try
                {
                    Buffer buffer = context.Device.OpenSharedResource<Buffer>(sharedHandle);
                    BufferDescription bdesc = buffer.Description;

                    if (!bdesc.OptionFlags.HasFlag(ResourceOptionFlags.StructuredBuffer))
                    {
                        buffer.Dispose();
                        throw new InvalidOperationException("This buffer handle does not have structured buffer flag");
                    }

                    this.Buffer = buffer;
                    this.Stride = bdesc.StructureByteStride;
                    this.ElementCount = bdesc.SizeInBytes / this.Stride;
                    this.Size = bdesc.SizeInBytes;

                    this.SRV = new ShaderResourceView(context.Device, this.Buffer);
                }
                catch
                {
                    throw new InvalidOperationException("Buffer handle is invalid");
                }             
            }

            protected override void OnDispose()
            {
                base.OnDispose();
                if (this.SRV != null)
                {
                    this.SRV.Dispose();
                }
            }
        }

        [Input("Resource Handle")]
        protected IDiffSpread<string> sharedHandle;

        [Output("Output")]
        protected Pin<DX11Resource<IDX11StructuredBuffer>> output;

        [Output("Is Valid")]
        protected ISpread<bool> isValid;

        protected bool invalidate;

        public void Evaluate(int SpreadMax)
        {
            if (this.sharedHandle.SliceCount == 0)
            {
                this.output.SafeDisposeAll();
                this.output.SliceCount = 0;
                return;
            }

            this.isValid.SliceCount = SpreadMax;
            this.output.SliceCount = SpreadMax;


            if (this.sharedHandle.IsChanged)
            {
                this.invalidate = true;
                this.output.SafeDisposeAll();
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.output[i] == null)
                {
                    output[i] = new DX11Resource<IDX11StructuredBuffer>();
                }
            }

        }

        public void Update(DX11RenderContext context)
        {
            if (this.invalidate)
            {
                for (int i = 0; i < this.output.SliceCount; i++)
                {
                    long handle;
                    long.TryParse(this.sharedHandle[i], out handle);
                    IntPtr handlePointer = new IntPtr(handle);

                    try
                    {
                        var buffer = new DX11SharedStructuredBuffer(context, handlePointer);
                        this.output[i][context] = buffer;
                        this.isValid[i] = true;
                    }
                    catch
                    {
                        this.output[i].Remove(context);
                        this.isValid[i] = false;
                    }
                }
                this.invalidate = false;
            }           
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.output.SafeDisposeAll(context);
        }

        public void Dispose()
        {
            this.output.SafeDisposeAll();
        }
    }
}
