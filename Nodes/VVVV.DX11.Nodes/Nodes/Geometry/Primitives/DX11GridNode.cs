using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using SlimDX.Direct3D11;
using SlimDX;


using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.Resources.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;
using FeralTic.DX11.Geometry;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Grid", Category = "DX11.Geometry", Version = "", Author = "vux")]
    public class DX11GridNode : DX11BasePrimitiveNode
    {
        [Input("Size", DefaultValues = new double[] { 1, 1 })]
        protected IDiffSpread<Vector2> FSize;

        [Input("Resolution X", DefaultValue=2, MinValue = 2)]
        protected IDiffSpread<int> FResX;

        [Input("Resolution Y", DefaultValue = 2, MinValue = 2)]
        protected IDiffSpread<int> FResY;

        protected override DX11IndexedGeometry GetGeom(DX11RenderContext context, int slice)
        {
            Grid grid = new Grid()
            {
                ResolutionX = this.FResX[slice],
                ResolutionY = this.FResY[slice],
                Size = this.FSize[slice]
            };

            return context.Primitives.Grid(grid);

        }

        protected override bool Invalidate()
        {
            return this.FSize.IsChanged || this.FResX.IsChanged || this.FResY.IsChanged;
        }
    }
}
