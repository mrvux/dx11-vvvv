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
    [PluginInfo(Name = "Torus", Category = "DX11.Geometry", Version = "", Author = "vux")]
    public class DX11TorusNode : DX11BasePrimitiveNode
    {
        [Input("Radius", DefaultValue = 0.5)]
        protected IDiffSpread<float> FSize;

        [Input("Thickness", DefaultValue = 0.1f)]
        protected IDiffSpread<float> FThick;

        [Input("Resolution X", DefaultValue = 15)]
        protected IDiffSpread<int> FResX;

        [Input("Resolution Y", DefaultValue = 15)]
        protected IDiffSpread<int> FResY;

        [Input("Phase X", DefaultValue = 1.0f)]
        protected IDiffSpread<float> FPX;

        [Input("Phase Y", DefaultValue = 1.0f)]
        protected IDiffSpread<float> FPY;

        [Input("Phase Rotation", DefaultValue = 1.0f)]
        protected IDiffSpread<float> FPR;

        [Input("Cycle Y", DefaultValue = 1.0f)]
        protected IDiffSpread<float> FCY;



        protected override DX11IndexedGeometry GetGeom(DX11RenderContext context, int slice)
        {
            Torus t = new Torus(this.FResX[slice], this.FResY[slice], this.FSize[slice], this.FThick[slice],
                this.FPY[slice], this.FPX[slice], this.FPR[slice], this.FCY[slice]);

            return context.Primitives.Torus(t);
        }

        protected override bool Invalidate()
        {
            return this.FSize.IsChanged || this.FResX.IsChanged || this.FResY.IsChanged
                || this.FThick.IsChanged || this.FPX.IsChanged || this.FPY.IsChanged || this.FPR.IsChanged
                || this.FCY.IsChanged;
        }
    }
}
