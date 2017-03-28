using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Resources;
using FeralTic.DX11;

using SlimDX;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "LineStrip", Category = "DX11.Geometry", Version = "3d", Author = "vux")]
    public class DX11LineStripNode : DX11BaseVertexPrimitiveNode
    {
        [Input("Vertices", DefaultValue = 0.0)]
        protected IDiffSpread<ISpread<Vector3>> FVerts;

        [Input("Loop", DefaultValue = 0.0)]
        protected IDiffSpread<bool> FLoop;

        [Input("Build Adjacency", DefaultValue = 0.0)]
        protected IDiffSpread<bool> FBuildAdjacency;

        protected override DX11VertexGeometry GetGeom(DX11RenderContext context, int slice)
        {
            return context.Primitives.LineStrip3d(this.FVerts[slice].ToList(), this.FLoop[slice], this.FBuildAdjacency[slice]);
        }

        protected override bool Invalidate()
        {
            return this.FVerts.IsChanged || this.FLoop.IsChanged || this.FBuildAdjacency.IsChanged;
        }

        protected override int GetSpreadMax(int spreadmax)
        {
            if (spreadmax == 0)
                return 0;
            if (this.FVerts.SliceCount == 0 || this.FLoop.SliceCount == 0) { return 0; }

            return Math.Max(this.FVerts.SliceCount, this.FLoop.SliceCount);
        }
    }
}
