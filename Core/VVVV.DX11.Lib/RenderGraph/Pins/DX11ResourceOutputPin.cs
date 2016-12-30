using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins.Output;
using SlimDX.Direct3D11;
using VVVV.Utils.Streams;
using VVVV.Hosting.Pins;
using VVVV.Utils.VMath;
using FeralTic.Resources;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Lib.RenderGraph.Pins
{
    public class DX11ResourceOutputStream<T, R> : MemoryIOStream<T>, IDisposable, IGenericIO
        where T : DX11Resource<R>, new()
        where R : IDX11Resource
    {
        private readonly INodeOut FNodeOut;

        public DX11ResourceOutputStream(INodeOut nodeOut, bool isSingle)
        {
            FNodeOut = nodeOut;
            FNodeOut.SetInterface(this);

            IConnectionHandler connectionHandler;
            if (isSingle)
            {
                connectionHandler = new DX11SingleResourceConnectionHandler(nodeOut);
            }
            else    
            {
                connectionHandler = new DX11ResourceConnectionHandler();
            }
            FNodeOut.SetConnectionHandler(connectionHandler, this);
        }

        object IGenericIO.GetSlice(int index)
        {
            return this[VMath.Zmod(index, Length)];
        }

        public override void Flush(bool force)
        {
            if (IsChanged)
            {
                FNodeOut.SliceCount = Length;
                FNodeOut.MarkPinAsChanged();
            }
            base.Flush();
        }

        protected override void BufferIncreased(T[] oldBuffer, T[] newBuffer)
        {
            Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
            if (oldBuffer.Length > 0)
            {
                for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
                newBuffer[i] = default(T);
            }
        }

        public void Dispose()
        {

        }
    }
}
