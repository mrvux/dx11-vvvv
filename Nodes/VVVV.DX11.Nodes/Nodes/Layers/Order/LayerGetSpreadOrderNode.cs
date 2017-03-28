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
    [PluginInfo(Name = "GetSpread", Category = "DX11.Layer", Version = "Order", Author = "vux", Tags = "layer")]
    public class LayerGetSpreadOrderNode : IPluginEvaluate
    {
        public class DX11LayerGetSpreadOrder : IDX11LayerOrder
        {
            private List<int> internalBuffer = new List<int>();

            public bool Enabled
            {
                get;
                set;
            }

            public ISpread<int> FInIndex { get; set; }
            public ISpread<int> FInCount { get; set; }

            public List<int> Reorder(DX11RenderSettings settings, List<DX11ObjectRenderSettings> objectSettings)
            {
                internalBuffer.Clear();
                int spreadMax = SpreadUtils.SpreadMax(this.FInCount, this.FInIndex);
                for (int i = 0; i < spreadMax; i++)
                {
                    int start = this.FInIndex[i];
                    int count = this.FInCount[i];
                    for (int j = 0; j < count; j++)
                    {
                        internalBuffer.Add(start + j);
                    }
                }
                return this.internalBuffer;
            }
        }

        [Input("Enabled", DefaultValue = 1)]
        protected ISpread<bool> FInEnabled;

        [Input("Index")]
        protected ISpread<int> FInIndex;

        [Input("Count", DefaultValue =1)]
        protected ISpread<int> FInCount;

        [Output("Output", IsSingle = true)]
        protected ISpread<DX11LayerGetSpreadOrder> FOut;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOut[0] == null) { this.FOut[0] = new DX11LayerGetSpreadOrder(); }

            this.FOut[0].Enabled = this.FInEnabled[0];
            this.FOut[0].FInIndex = this.FInIndex;
            this.FOut[0].FInCount = this.FInCount;
        }
    }


}
