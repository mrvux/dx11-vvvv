using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Cylinder", Category = "DX11.Geometry", Version = "", Author = "vux")]
    public class DX11CylinderNode : DX11BasePrimitiveNode
    {
        [Input("Radius 1", DefaultValue=0.5)]
        protected IDiffSpread<float> FInR1;

        [Input("Radius 2", DefaultValue = 0.5)]
        protected IDiffSpread<float> FInR2;

        [Input("Length", DefaultValue = 1.0)]
        protected IDiffSpread<float> FInLength;

        [Input("Cycles", DefaultValue = 1.0)]
        protected IDiffSpread<float> FInCycles;

        [Input("Caps", DefaultValue = 1.0)]
        protected IDiffSpread<bool> FInCaps;

        [Input("Resolution X", DefaultValue = 15)]
        protected IDiffSpread<int> FInResX;

        [Input("Resolution Y", DefaultValue = 1)]
        protected IDiffSpread<int> FInResY;

        protected override DX11IndexedGeometry GetGeom(DX11RenderContext context, int slice)
        {
            return context.Primitives.Cylinder(this.FInR1[slice], this.FInR2[slice],this.FInCycles[slice], this.FInLength[slice],
                this.FInResX[slice], this.FInResY[slice], this.FInCaps[slice]);
        }

        protected override bool Invalidate()
        {
            return this.FInCaps.IsChanged
                || this.FInCycles.IsChanged
                || this.FInLength.IsChanged
                || this.FInR1.IsChanged
                || this.FInR2.IsChanged
                || this.FInResX.IsChanged
                || this.FInResY.IsChanged;
        }
    }
}
