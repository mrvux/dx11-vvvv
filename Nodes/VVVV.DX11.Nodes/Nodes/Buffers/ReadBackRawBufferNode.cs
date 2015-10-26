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
    [PluginInfo(Name = "ReadBack", Category = "DX11.Buffer", Version = "Raw", Author = "vux", AutoEvaluate = false)]
    public class ReadBackRawBuffer : IPluginEvaluate, IDX11ResourceDataRetriever, IPartImportsSatisfiedNotification
    {
        [Config("Layout")]
        protected IDiffSpread<string> FLayout;

        [Input("Stride",DefaultValue=24)]
        protected Pin<int> FInStride;

        [Input("Input")]
        protected Pin<DX11Resource<DX11RawBuffer>> FInput;

        List<IIOContainer> outspreads = new List<IIOContainer>();
        string[] layout;

        [Import()]
        protected IPluginHost FHost;

        [Import()]
        protected IIOFactory FIO;

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;


        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            if (this.FInput.IsConnected)
            {
                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                DX11RawBuffer b = this.FInput[0][this.AssignedContext];

                if (b != null)
                {
                    DX11StagingRawBuffer staging = new DX11StagingRawBuffer(this.AssignedContext.Device, b.Size);

                    this.AssignedContext.CurrentDeviceContext.CopyResource(b.Buffer, staging.Buffer);
                    int elem = b.Size / this.FInStride[0];
                    foreach (IIOContainer sp in this.outspreads)
                    {
                        ISpread s = (ISpread)sp.RawIOObject;
                        s.SliceCount = elem;
                    }

                    DataStream ds = staging.MapForRead(this.AssignedContext.CurrentDeviceContext);

                    for (int i = 0; i < elem; i++)
                    {
                        int cnt = 0;
                        foreach (string lay in layout)
                        {
                            switch (lay)
                            {
                                case "float":
                                    ISpread<float> spr = (ISpread<float>)this.outspreads[cnt].RawIOObject;
                                    spr[i] = ds.Read<float>();
                                    break;
                                case "float2":
                                    ISpread<Vector2> spr2 = (ISpread<Vector2>)this.outspreads[cnt].RawIOObject;
                                    spr2[i] = ds.Read<Vector2>();
                                    break;
                                case "float3":
                                    ISpread<Vector3> spr3 = (ISpread<Vector3>)this.outspreads[cnt].RawIOObject;
                                    spr3[i] = ds.Read<Vector3>();
                                    break;
                                case "float4":
                                    ISpread<Vector4> spr4 = (ISpread<Vector4>)this.outspreads[cnt].RawIOObject;
                                    spr4[i] = ds.Read<Vector4>();
                                    break;
                                case "float4x4":
                                    ISpread<Matrix> sprm = (ISpread<Matrix>)this.outspreads[cnt].RawIOObject;
                                    sprm[i] = ds.Read<Matrix>();
                                    break;
                                case "int":
                                    ISpread<int> spri = (ISpread<int>)this.outspreads[cnt].RawIOObject;
                                    spri[i] = ds.Read<int>();
                                    break;
                                case "uint":
                                    ISpread<uint> sprui = (ISpread<uint>)this.outspreads[cnt].RawIOObject;
                                    sprui[i] = ds.Read<uint>();
                                    break;
                                case "uint2":
                                    ISpread<Vector2> sprui2 = (ISpread<Vector2>)this.outspreads[cnt].RawIOObject;
                                    uint ui1 = ds.Read<uint>();
                                    uint ui2 = ds.Read<uint>();
                                    sprui2[i] = new Vector2(ui1, ui2);
                                    break;
                                case "uint3":
                                    ISpread<Vector3> sprui3 = (ISpread<Vector3>)this.outspreads[cnt].RawIOObject;
                                    uint ui31 = ds.Read<uint>();
                                    uint ui32 = ds.Read<uint>();
                                    uint ui33 = ds.Read<uint>();
                                    sprui3[i] = new Vector3(ui31, ui32, ui33);
                                    break;
                            }
                            cnt++;
                        }

                    }

                    staging.UnMap(this.AssignedContext.CurrentDeviceContext);

                    staging.Dispose();
                }
                else
                {
                    foreach (IIOContainer sp in this.outspreads)
                    {
                        ISpread s = (ISpread)sp.RawIOObject;
                        s.SliceCount = 0;
                    }
                }
            }
            else
            {
                foreach (IIOContainer sp in this.outspreads)
                {
                    ISpread s = (ISpread)sp.RawIOObject;
                    s.SliceCount = 0;
                }
            }
        }
        #endregion

        public void OnImportsSatisfied()
        {
            this.FLayout.Changed += new SpreadChangedEventHander<string>(FLayout_Changed);
        }

        void FLayout_Changed(IDiffSpread<string> spread)
        {
            foreach (IIOContainer sp in this.outspreads)
            {
                sp.Dispose();
            }
            this.outspreads.Clear();

            layout = spread[0].Split(",".ToCharArray());

            int id = 1;

            foreach (string lay in layout)
            {
                OutputAttribute attr = new OutputAttribute("Output " + id.ToString());
                IIOContainer container = null;
                switch (lay)
                {
                    case "float":
                        container = this.FIO.CreateIOContainer<ISpread<float>>(attr);
                        break;
                    case "float2":
                        container = this.FIO.CreateIOContainer<ISpread<Vector2>>(attr);
                        break;
                    case "float3":
                        container = this.FIO.CreateIOContainer<ISpread<Vector3>>(attr);
                        break;
                    case "float4":
                        container = this.FIO.CreateIOContainer<ISpread<Vector4>>(attr);
                        break;
                    case "float4x4":
                        container = this.FIO.CreateIOContainer<ISpread<Matrix>>(attr);
                        break;
                    case "int":
                        container = this.FIO.CreateIOContainer<ISpread<int>>(attr);
                        break;
                    case "uint":
                        container = this.FIO.CreateIOContainer<ISpread<uint>>(attr);
                        break;
                    case "uint2":
                        //attr.AsInt = true;
                        container = this.FIO.CreateIOContainer<ISpread<Vector2>>(attr);
                        break;
                    case "uint3":
                        //attr.AsInt = true;
                        container = this.FIO.CreateIOContainer<ISpread<Vector3>>(attr);
                        break;
                }

                if (container != null) { this.outspreads.Add(container); id++; }
            }
        }
    }
}
