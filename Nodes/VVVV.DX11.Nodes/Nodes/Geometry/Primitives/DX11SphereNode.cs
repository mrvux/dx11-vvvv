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
    [PluginInfo(Name = "Sphere", Category = "DX11.Geometry", Version = "", Author = "vux")]
    public class DX11SphereNode : DX11BasePrimitiveNode
    {
        [Input("Radius", DefaultValue = 0.5)]
        protected IDiffSpread<float> FSize;

        [Input("Cycles X", DefaultValue = 1.0f)]
        protected IDiffSpread<float> FCyclesX;

        [Input("Cycles Y", DefaultValue = 1.0f)]
        protected IDiffSpread<float> FCyclesY;

        [Input("Resolution X", DefaultValue = 15)]
        protected IDiffSpread<int> FResX;

        [Input("Resolution Y", DefaultValue = 15)]
        protected IDiffSpread<int> FResY;


        protected override DX11IndexedGeometry GetGeom(DX11RenderContext context, int slice)
        {
            Sphere sphere = new Sphere();
            sphere.CyclesX = this.FCyclesX[slice];
            sphere.CyclesY = this.FCyclesY[slice];
            sphere.Radius = this.FSize[slice];
            sphere.ResX = this.FResX[slice];
            sphere.ResY = this.FResY[slice];

            return context.Primitives.Sphere(sphere);
        }

        protected override bool Invalidate()
        {
            return this.FSize.IsChanged || this.FResX.IsChanged || this.FResY.IsChanged
                || this.FCyclesY.IsChanged || this.FCyclesX.IsChanged;
        }
    }
}
