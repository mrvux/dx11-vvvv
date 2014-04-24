using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using SlimDX.Direct3D11;
using SlimDX;

using FeralTic.DX11.Resources;
using FeralTic.DX11;
using FeralTic.DX11.Geometry;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "IcoSphere", Category = "DX11.Geometry", Version = "", Author = "vux",Credits="Alexandre Mutel, Sharpdx")]
    public class DX11IcoSphereNode : DX11BasePrimitiveNode
    {
        [Input("Radius", DefaultValue = 0.5)]
        protected IDiffSpread<float> FSize;

        [Input("SubDivisions", DefaultValue = 1)]
        protected IDiffSpread<int> FSubDiv;

        protected override DX11IndexedGeometry GetGeom(DX11RenderContext context, int slice)
        {
            IcoSphere sph = new IcoSphere();
            sph.Radius = this.FSize[slice];
            sph.SubDivisions = this.FSubDiv[slice];

            return context.Primitives.IcoSphere(sph);
        }

        protected override bool Invalidate()
        {
            return this.FSize.IsChanged || this.FSubDiv.IsChanged;
        }
    }
}
