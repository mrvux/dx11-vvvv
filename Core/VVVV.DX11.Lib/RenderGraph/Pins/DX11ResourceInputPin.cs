using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins;
using VVVV.Utils.Streams;
using FeralTic.Resources;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Lib.RenderGraph.Pins
{
    public class DX11ResourceInputStream<T, R> : MemoryIOStream<T>, IDisposable
        where T : DX11Resource<R>, new()
        where R : IDX11Resource
    {
        private readonly INodeIn FNodeIn;
        private readonly bool FAutoValidate;

        public DX11ResourceInputStream(INodeIn nodeIn)
        {
            FNodeIn = nodeIn;
            FNodeIn.SetConnectionHandler(new DX11ResourceConnectionHandler(), this);
            FAutoValidate = nodeIn.AutoValidate;
        }


        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FNodeIn.PinIsChanged : FNodeIn.Validate();
            if (IsChanged)
            {
                Length = FNodeIn.SliceCount;
                using (var writer = GetWriter())
                {
                    object usI;
                    FNodeIn.GetUpstreamInterface(out usI);
                    var upstreamInterface = usI as IGenericIO;

                    for (int i = 0; i < Length; i++)
                    {
                        int usS;
                        T result = new T();
                        if (upstreamInterface != null)
                        {
                            FNodeIn.GetUpsreamSlice(i, out usS);
                            IDX11ResourceDataProvider res = (IDX11ResourceDataProvider)upstreamInterface.GetSlice(usS);

                            if (result == null) { res = new T(); }
                            result.Assign(res);
                        }
                        writer.Write(result);
                    }
                }
            }
            return base.Sync();
        }

        public void Dispose()
        {
            
        }
    }
}
