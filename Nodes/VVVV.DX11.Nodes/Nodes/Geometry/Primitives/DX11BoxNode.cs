using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using SlimDX.Direct3D11;
using SlimDX;

using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Box", Category = "DX11.Geometry", Version = "", Author = "vux")]
    public class DX11BoxNode : DX11BasePrimitiveNode
    {
        [Input("Size",DefaultValues= new double[] { 1,1,1})]
        protected IDiffSpread<Vector3> FSize;

        private Box settings = new Box();

        protected override DX11IndexedGeometry GetGeom(DX11RenderContext context, int slice)
        {
            settings.Size = this.FSize[slice];
            return context.Primitives.Box(settings);
        }

        protected override bool Invalidate()
        {
            return this.FSize.IsChanged;
        }
    }
}
