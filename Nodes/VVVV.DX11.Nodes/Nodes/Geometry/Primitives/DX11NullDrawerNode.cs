using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "NullGeometry", Category = "DX11.Drawer", Version = "", Author = "vux")]
    public class DX11NullDrawerNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Vertex Count", DefaultValue=1, MinValue=1)]
        protected IDiffSpread<int> FInVertexCount;

        [Input("Instance Count", DefaultValue=1,MinValue=1)]
        protected IDiffSpread<int> FInInstanceCount;

        [Input("Topology", Order = 2, DefaultEnumEntry = "PointList")]
        protected IDiffSpread<PrimitiveTopology> FInTopology;

        [Output("Geometry Out", Order = 5)]
        protected Pin<DX11Resource<DX11NullGeometry>> FOutput;

        private bool FInvalidate;

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            this.FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutput[i] == null)
                {
                    this.FOutput[i] = new DX11Resource<DX11NullGeometry>();
                }
            }

            if (this.FInVertexCount.IsChanged || this.FInInstanceCount.IsChanged || this.FInTopology.IsChanged)
            {
                this.FInvalidate = true;
            }
        }

        public void Update(DX11RenderContext context)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                if (this.FInvalidate || !this.FOutput[i].Contains(context))
                {
                    if (this.FOutput[i].Contains(context)) { this.FOutput[i].Dispose(context); }

                    DX11NullInstancedDrawer drawer = new DX11NullInstancedDrawer();
                    drawer.VertexCount = Math.Max(this.FInVertexCount[i], 1);
                    drawer.InstanceCount = Math.Max(this.FInInstanceCount[i], 1);

                    DX11NullGeometry geom = new DX11NullGeometry(context, drawer);
                    geom.Topology = this.FInTopology[i];
                    geom.InputLayout = new InputElement[0];
                    geom.HasBoundingBox = false;

                    this.FOutput[i][context] = geom;
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            
        }
    }
}
