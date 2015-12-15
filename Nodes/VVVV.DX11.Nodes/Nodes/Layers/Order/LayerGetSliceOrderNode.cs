using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using VVVV.PluginInterfaces.V2;

using FeralTic.Resources.Geometry;

using VVVV.DX11.Validators;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "GetSlice", Category = "DX11.Layer", Version = "Order", Author = "vux", Tags = "layer")]
    public class LayerGetSliceOrderNode : IPluginEvaluate
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

        [Input("Enabled", DefaultValue = 1)]
        protected ISpread<bool> FInEnabled;

        [Input("Index")]
        protected ISpread<int> FInIndex;

        [Output("Output", IsSingle = true)]
        protected ISpread<DX11LayerGetSliceOrder> FOut;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOut[0] == null) { this.FOut[0] = new DX11LayerGetSliceOrder(); }

            this.FOut[0].Enabled = this.FInEnabled[0];
            this.FOut[0].FInIndex = this.FInIndex;
        }
    }


}
