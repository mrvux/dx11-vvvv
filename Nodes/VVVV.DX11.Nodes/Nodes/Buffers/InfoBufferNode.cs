using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Info", Category = "DX11.Buffer", Version = "", Author = "vux", AutoEvaluate=false)]
    public class InfoBufferNode : IPluginEvaluate, IDX11ResourceDataRetriever
    {
        [Input("Buffer In")]
        protected Pin<DX11Resource<IDX11Buffer>> FBufferIn;

        [Output("Size")]
        protected ISpread<int> FOutSize;

        [Output("Stride")]
        protected ISpread<int> FOutStride;

        [Output("Is Structured")]
        protected ISpread<bool> FOutIsStructured;

        [Output("Is Raw")]
        protected ISpread<bool> FOutIsRaw;

        [Output("Resource Pointer", Visibility=PinVisibility.OnlyInspector)]
        protected ISpread<int> FOutPointer;

        [Output("Creation Time", Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FOutCreationTime;

        [Import()]
        protected IPluginHost FHost;

        public DX11RenderContext AssignedContext
        {
            get;
            set; 
        }

        public event DX11RenderRequestDelegate RenderRequest;


        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            if (this.FBufferIn.IsConnected)
            {
                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null) { this.SetNull(); return; }
                //Do NOT cache this, assignment done by the host

                this.FOutSize.SliceCount = this.FBufferIn.SliceCount;
                this.FOutStride.SliceCount = this.FBufferIn.SliceCount;
                this.FOutIsStructured.SliceCount = this.FBufferIn.SliceCount;
                this.FOutIsRaw.SliceCount = this.FBufferIn.SliceCount;
                this.FOutPointer.SliceCount = this.FBufferIn.SliceCount;
                this.FOutCreationTime.SliceCount = this.FBufferIn.SliceCount;

                for (int i = 0; i < this.FBufferIn.SliceCount; i++)
                {
                    try
                    {
                        if (this.FBufferIn[i].Contains(this.AssignedContext))
                        {
                            if (this.FBufferIn[i][this.AssignedContext] != null)
                            {
                                BufferDescription tdesc = this.FBufferIn[i][this.AssignedContext].Buffer.Description;
                                this.FOutSize[i] = tdesc.SizeInBytes;
                                this.FOutStride[i] = tdesc.StructureByteStride;
                                this.FOutIsStructured[i] = tdesc.OptionFlags.HasFlag(ResourceOptionFlags.StructuredBuffer);
                                this.FOutIsRaw[i] = tdesc.OptionFlags.HasFlag(ResourceOptionFlags.RawBuffer);
                                this.FOutPointer[i] = this.FBufferIn[i][this.AssignedContext].Buffer.ComPointer.ToInt32();
                                this.FOutCreationTime[i] = this.FBufferIn[i][this.AssignedContext].Buffer.CreationTime;
                            }
                            else
                            {
                                this.SetDefault(i);
                            }
                        }
                        else
                        {
                            this.SetDefault(i);
                        }
                    }
                    catch
                    {
                        this.SetDefault(i);
                    }
                }
            }
            else
            {
                this.SetNull();
            }
        }

        #endregion

        private void SetNull()
        {
            this.FOutSize.SliceCount = 0;
            this.FOutStride.SliceCount = 0;
            this.FOutIsStructured.SliceCount = 0;
            this.FOutIsRaw.SliceCount = 0;
            this.FOutPointer.SliceCount = 0;
            this.FOutCreationTime.SliceCount = 0;
        }

        private void SetDefault(int i)
        {
            this.FOutSize[i] = -1;
            this.FOutStride[i] = -1;
            this.FOutIsStructured[i] = false;
            this.FOutIsRaw[i] = false;
            this.FOutPointer[i] = -1;
            this.FOutCreationTime[i] = 0;
        }




    }
}
