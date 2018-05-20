using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using SlimDX.Direct3D11;
using System.ComponentModel.Composition;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes.Textures
{
    [PluginInfo(Name = "AsSharedResource", Category = "DX11.Buffer", Version = "", Author = "vux", AutoEvaluate =true,
        Help = "Creates a shared buffer to allow cross process buffer sharing. Please not that an internal copy of the buffer is created")]
    public class ToSharedBufferNode : IPluginEvaluate, IDX11ResourceDataRetriever, IDisposable
    {
        [Import()]
        protected IPluginHost FHost;

        [Input("Buffer In", IsSingle=true)]
        protected Pin<DX11Resource<IDX11Buffer>> FBufferIn;

        [Input("Flush", IsSingle = true)]
        protected ISpread<bool> flush;

        [Output("Resource Handle",IsSingle=true)]
        protected ISpread<string> FPointer;

        private SlimDX.Direct3D11.Buffer currentBuffer = null;
        private SlimDX.DXGI.Resource sharedResource = null;
        private SlimDX.Direct3D11.BufferDescription currentDescription;

        public void Evaluate(int SpreadMax)
        {
            if (this.FBufferIn.IsConnected)
            {

                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null) { this.SetNull(); return; }

                this.FPointer.SliceCount = SpreadMax;

                DX11RenderContext context = this.AssignedContext;


                try
                {
                    if (this.FBufferIn[0].Contains(context))
                    {
                        var inputBuffer = this.FBufferIn[0][context].Buffer;

                        if (this.currentBuffer != null)
                        {
                            if (this.currentDescription != inputBuffer.Description)
                            {
                                this.currentBuffer.Dispose();
                                this.currentBuffer = null;
                                this.sharedResource.Dispose();
                                this.sharedResource = null;
                            }
                        }

                        //As it can come from dynamic, make sure to allow share put in default pool and deny cpu access
                        if (this.currentBuffer == null)
                        {
                            var desc = inputBuffer.Description;

                            desc.BindFlags = BindFlags.ShaderResource; //This is important, as resource needs read access later on, it's not only a copy, if not set other side will not be able to create srv
                            desc.OptionFlags |= ResourceOptionFlags.Shared;
                            desc.Usage = ResourceUsage.Default;
                            desc.CpuAccessFlags = CpuAccessFlags.None;

                            this.currentBuffer = new SlimDX.Direct3D11.Buffer(context.Device, desc);
                            this.sharedResource = new SlimDX.DXGI.Resource(this.currentBuffer);
                            this.currentDescription = inputBuffer.Description;

                            this.FPointer[0] = this.sharedResource.SharedHandle.ToString();
                        }

                        this.AssignedContext.CurrentDeviceContext.CopyResource(inputBuffer, this.currentBuffer);

                        if (this.flush[0])
                        {
                            this.AssignedContext.CurrentDeviceContext.Flush();
                        }
                    }
                    else
                    {
                        this.SetDefault(0);
                    }
                }
                catch
                {
                    this.SetDefault(0);
                }
            }
            else
            {
                this.SetNull();
            }
        }


        private void SetNull()
        {
            this.FPointer.SliceCount = 0;
        }

        private void SetDefault(int i)
        {
            this.FPointer[i] = "";
        }

        public void Dispose()
        {
            try
            {
                this.sharedResource?.Dispose();
                this.currentBuffer?.Dispose();
            }
            catch
            {
            }
        }

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;
    }
}
