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
    [PluginInfo(Name = "ZSort", Category = "DX11.Layer", Version = "Order", Author = "vux", Tags = "layer")]
    public class LayerZSortOrderNode : IPluginEvaluate
    {
        public class DX11Zsort : IDX11LayerOrder
        {
            private class IndexedTransform
            {
                public float Z;
                public int Index;
            }

            private class IndexedTransformComparer : IComparer<IndexedTransform>
            {
                public int Compare(IndexedTransform x, IndexedTransform y)
                {
                    return y.Z.CompareTo(x.Z);
                }
            }

            private List<int> internalBuffer = new List<int>();

            private List<IndexedTransform> indexedTransform = new List<IndexedTransform>();
            private IndexedTransformComparer comparer = new IndexedTransformComparer();

            public List<int> InternalBuffer
            {
                get { return this.internalBuffer; }
            }

            public bool Enabled
            {
                get;
                set;
            }

            public List<int> Reorder(DX11RenderSettings settings, List<DX11ObjectRenderSettings> objectSettings)
            {
                internalBuffer.Clear();
                indexedTransform.Clear();

                for (int i = 0; i < objectSettings.Count; i++)
                {
                    Matrix world = objectSettings[i].WorldTransform;
                    Vector3 pos = new Vector3(world.M41, world.M42, world.M43);
                    indexedTransform.Add(new IndexedTransform()
                        {
                            Index = i,
                            Z = Vector3.TransformCoordinate(pos, settings.View).Z
                        });
                }

                indexedTransform.Sort(this.comparer);

                for (int i = 0; i < indexedTransform.Count; i++)
                {
                    internalBuffer.Add(indexedTransform[i].Index);
                }
                return this.internalBuffer;
            }
        }

        [Input("Enabled", DefaultValue = 1)]
        protected ISpread<bool> FInEnabled;

        [Output("Output", IsSingle = true)]
        protected ISpread<DX11Zsort> FOut;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOut[0] == null) { this.FOut[0] = new DX11Zsort(); }

            this.FOut[0].Enabled = this.FInEnabled[0];
        }
    }


}
