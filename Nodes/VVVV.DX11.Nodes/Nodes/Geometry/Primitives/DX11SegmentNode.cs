using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using SlimDX.Direct3D11;
using SlimDX;

using FeralTic.Resources.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Segment", Category = "DX11.Geometry", Version = "", Author = "vux")]
    public class DX11SegmentNode : DX11BasePrimitiveNode
    {
        [Input("Phase", DefaultValue = 0)]
        protected IDiffSpread<float> FInPhase;

        [Input("Inner Radius", DefaultValue =0)]
        protected IDiffSpread<float> FInInner;

        [Input("Cycles", DefaultValue = 1)]
        protected IDiffSpread<float> FInCycles;

        [Input("Flat Texture", DefaultValue = 1)]
        protected IDiffSpread<bool> FInFlat;

        [Input("Resolution", DefaultValue = 20)]
        protected IDiffSpread<int> FInRes;

        protected override DX11IndexedGeometry GetGeom(DX11RenderContext context, int slice)
        {
            return context.Primitives.Segment(this.FInPhase[slice], this.FInCycles[slice], this.FInInner[slice], this.FInRes[slice], this.FInFlat[slice]);
        }

        protected override bool Invalidate()
        {
            return this.FInCycles.IsChanged
                || this.FInFlat.IsChanged
                || this.FInInner.IsChanged
                || this.FInPhase.IsChanged
                || this.FInRes.IsChanged;
        }
    }
}
