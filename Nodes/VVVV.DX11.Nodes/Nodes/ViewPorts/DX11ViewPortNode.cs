using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "ViewPort", Category = "DX11", Author = "vux")]
    public class ViewPortNode : IPluginEvaluate
    {
        [Input("Center")]
        protected ISpread<Vector2> FInPosition;

        [Input("Size", DefaultValues = new double[] { 1.0,1.0})]
        protected ISpread<Vector2> FInSize;

        [Input("Min Z", DefaultValue = 0.0)]
        protected ISpread<float> FInMinZ;

        [Input("Max Z", DefaultValue = 1.0)]
        protected ISpread<float> FInMaxZ;

        [Output("ViewPort")]
        protected ISpread<Viewport> FViewPort;

        public void Evaluate(int SpreadMax)
        {
            this.FViewPort.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                Viewport vp = new Viewport();
                vp.Height = this.FInSize[i].Y;
                vp.MaxZ = this.FInMaxZ[i];
                vp.MinZ = this.FInMinZ[i];
                vp.Width = this.FInSize[i].X;
                vp.X = this.FInPosition[i].X;
                vp.Y = this.FInPosition[i].Y;
                this.FViewPort[i] = vp;
            }
        }


    }
}
