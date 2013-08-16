using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using SlimDX.Direct3D11;
using SlimDX;

using FeralTic.DX11.Resources;
using FeralTic.DX11;
using FeralTic.DX11.Geometry;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "QuadCross", Category = "DX11.Geometry", Version = "", Author = "vux")]
    public class DX11QuadCrossNode : DX11BasePrimitiveNode
    {
        [Input("Size",DefaultValues= new double[] { 1,1})]
        protected IDiffSpread<Vector2> FSize;

        protected override DX11IndexedGeometry GetGeom(DX11RenderContext context, int slice)
        {
            Quad quad = new Quad();
            quad.Size = this.FSize[slice];

            return context.Primitives.QuadCross(quad);
        }

        protected override bool Invalidate()
        {
            return this.FSize.IsChanged;
        }

    }
}
