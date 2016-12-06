using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using SlimDX.Direct3D11;
using SlimDX;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name="Scissor",Category="DX11.Layer",Version="", Author="vux")]
    public class DX11LayerScissorNode : IPluginEvaluate, IDX11LayerHost
    {
        [Input("Position")]
        protected ISpread<Vector2> FInPosition;

        [Input("Size")]
        protected ISpread<Vector2> FInSize;

        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Enabled",DefaultValue=1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        private System.Drawing.Rectangle[] rectangles = new System.Drawing.Rectangle[0];

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }

            if (rectangles.Length != SpreadMax)
            {
                rectangles = new System.Drawing.Rectangle[SpreadMax];
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                int px ,py,sx,sy;
                px = (int)FInPosition[i].X;
                py = (int)FInPosition[i].Y;
                sx = (int)FInSize[i].X;
                sy = (int)FInSize[i].Y;

                rectangles[i] = new System.Drawing.Rectangle(px, py, sx, sy);
            }
        }


        #region IDX11ResourceProvider Members

        public void Update(DX11RenderContext context)
        {
            if (!this.FOutLayer[0].Contains(context))
            {
                this.FOutLayer[0][context] = new DX11Layer();
                this.FOutLayer[0][context].Render = this.Render;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutLayer[0].Dispose(context);
        }

        public void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.FEnabled[0])
            {
                var rect = context.CurrentDeviceContext.Rasterizer.GetScissorRectangles();
                if (this.FLayerIn.IsConnected)
                {

                    context.CurrentDeviceContext.Rasterizer.SetScissorRectangles(this.rectangles);
                    try
                    {
                        for (int i = 0; i < this.FLayerIn.SliceCount; i++)
                        {
                            this.FLayerIn.RenderAll(context, settings);
                        }
                    }
                    finally
                    {
                        context.CurrentDeviceContext.Rasterizer.SetScissorRectangles(rect);
                    }
                }
            }
            else
            {
                if (this.FLayerIn.IsConnected)
                {
                    this.FLayerIn.RenderAll(context, settings);
                }
            }
        }

        #endregion
    }
}
