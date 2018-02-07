using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Resources;
using FeralTic.DX11;

using SlimDX.Direct3D11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "S+H", Category = "DX11.Texture", Version = "2d", Author = "vux", Help = "Sample and Hold - if set is 1 just passes the input through(makes a copy of it), when set is 0 keeps the copy instead(and block upwards evaluation and rendering")]
    public class SampleHoldTexture2DNode : SampleHoldResourceNode<DX11Texture2D>
    {
        private Texture2DDescription currentDescription;

        protected override void ProcessResource(DX11RenderContext context, DX11Texture2D inputResource, ref DX11Texture2D outputResource)
        {
            if (outputResource != null)
            {
                if (currentDescription != outputResource.Description)
                {
                    outputResource.Dispose();
                    outputResource = null;
                }
                
            }

            if (outputResource == null)
            {
                this.currentDescription = inputResource.Description;

                /*make default pool, disallow cpu access and only allow shader view flag (as we will not write to it) */
                Texture2DDescription desc = this.currentDescription;
                desc.Usage = ResourceUsage.Default;
                desc.BindFlags = BindFlags.ShaderResource;
                desc.CpuAccessFlags = CpuAccessFlags.None;

                outputResource = DX11Texture2D.FromDescription(context, desc);
            }
            context.CurrentDeviceContext.CopyResource(inputResource.Resource, outputResource.Resource);
        }
    }

    [PluginInfo(Name = "S+H", Category = "DX11.Buffer", Version = "Structured", Author = "vux", Help ="Sample and Hold - if set is 1 just passes the input through(makes a copy of it), when set is 0 keeps the copy instead(and block upwards evaluation and rendering")]
    public class SampleHoldStructuredNode : SampleHoldResourceNode<IDX11ReadableStructureBuffer>
    {
        private BufferDescription currentDescription;

        protected override void ProcessResource(DX11RenderContext context, IDX11ReadableStructureBuffer inputResource, ref IDX11ReadableStructureBuffer outputResource)
        {
            if (outputResource != null)
            {
                if (currentDescription != outputResource.Buffer.Description)
                {
                    outputResource.Dispose();
                    outputResource = null;
                }
            }

            if (outputResource == null)
            {
                this.currentDescription = inputResource.Buffer.Description;
                BufferDescription desc = this.currentDescription;
                desc.Usage = ResourceUsage.Default;
                desc.BindFlags = BindFlags.ShaderResource;
                desc.CpuAccessFlags = CpuAccessFlags.None;

                outputResource = new DX11CopyDestStructuredBuffer(context.Device, desc);
            }
            context.CurrentDeviceContext.CopyResource(inputResource.Buffer, outputResource.Buffer);
        }
    }

    [PluginInfo(Name = "S+H", Category = "DX11.Texture", Version = "3d", Author = "vux", Help = "Sample and Hold - if set is 1 just passes the input through(makes a copy of it), when set is 0 keeps the copy instead(and block upwards evaluation and rendering")]
    public class SampleHoldTexture3DNode : SampleHoldResourceNode<DX11Texture3D>
    {
        private Texture3DDescription currentDescription;

        protected override void ProcessResource(DX11RenderContext context, DX11Texture3D inputResource, ref DX11Texture3D outputResource)
        {
            if (outputResource != null)
            {
                if (currentDescription != outputResource.Resource.Description)
                {
                    outputResource.Dispose();
                    outputResource = null;
                }

            }

            if (outputResource == null)
            {
                this.currentDescription = inputResource.Resource.Description;

                Texture3DDescription desc = this.currentDescription;
                desc.Usage = ResourceUsage.Default;
                desc.BindFlags = BindFlags.ShaderResource;
                desc.CpuAccessFlags = CpuAccessFlags.None;

                outputResource = DX11Texture3D.FromDescription(context, desc);
            }
            context.CurrentDeviceContext.CopyResource(inputResource.Resource, outputResource.Resource);
        }
    }



}
