using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using VVVV.PluginInterfaces.V2;

using FeralTic.Resources.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "RoundRect", Category = "DX11.Geometry", Version = "", Author = "vux")]
    public class DX11RoundRectNode : DX11BasePrimitiveNode
    {
        [Input("Inner Radius", DefaultValues = new double[] { 0.35, 0.35 })]
        protected IDiffSpread<Vector2> FInInner;

        [Input("Outer Radius", DefaultValue = 0.15)]
        protected IDiffSpread<float> FInOuter;

        [Input("Corner Resolution", DefaultValue = 20)]
        protected IDiffSpread<int> FInRes;

        protected override DX11IndexedGeometry GetGeom(DX11RenderContext context, int slice)
        {
            return context.Primitives.RoundRect(this.FInInner[slice],this.FInOuter[slice],this.FInRes[slice]);
        }

        protected override bool Invalidate()
        {
            return this.FInInner.IsChanged
                || this.FInOuter.IsChanged
                || this.FInRes.IsChanged;
        }
    }
}
