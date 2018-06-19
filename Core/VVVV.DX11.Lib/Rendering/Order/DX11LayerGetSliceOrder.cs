using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Lib
{
    public class DX11LayerGetSliceOrder : IDX11LayerOrder
    {
        private List<int> internalBuffer = new List<int>();

        public bool Enabled
        {
            get;
            set;
        }

        public ISpread<int> FInIndex { get; set; }

        public List<int> Reorder(DX11RenderSettings settings, List<DX11ObjectRenderSettings> objectSettings)
        {
            internalBuffer.Clear();
            for (int i = 0; i < FInIndex.SliceCount; i++)
            {
                internalBuffer.Add(FInIndex[i]);
            }
            return this.internalBuffer;
        }
    }
}
