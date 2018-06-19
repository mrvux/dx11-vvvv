using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.Lib
{
    public class DX11SingleSliceOrder : IDX11LayerOrder
    {
        private List<int> internalBuffer = new List<int>();

        public DX11SingleSliceOrder()
        {
            this.Enabled = true;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public int FInIndex;

        public List<int> Reorder(DX11RenderSettings settings, List<DX11ObjectRenderSettings> objectSettings)
        {
            internalBuffer.Clear();
            internalBuffer.Add(this.FInIndex);
            return this.internalBuffer;
        }
    }
}
