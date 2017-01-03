using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using SlimDX.Direct3D11;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Viewport", Category = "DX11.Layer", Version = "Indexed", Author = "vux")]
    public class DX11LayerViewportIndexedNode : IPluginEvaluate, IDX11LayerHost
    {
        [Input("Viewports")]
        protected Pin<Viewport> FInViewports;

        [Input("Normalized")]
        protected ISpread<bool> FNormalized;

        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Enabled", DefaultValue = 1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }
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
            if (this.FEnabled[0] && this.FInViewports.SliceCount > 0 && this.FNormalized.SliceCount > 0)
            {
                if (this.FLayerIn.IsConnected)
                {
                    bool enabled = this.FInViewports.PluginIO.IsConnected;

                    if (enabled)
                    {
                        var vp = this.FInViewports[settings.ViewportIndex];
                        if (FNormalized[settings.ViewportIndex])
                        {
                            vp = vp.Normalize(settings.RenderWidth, settings.RenderHeight);
                        }

                        context.CurrentDeviceContext.Rasterizer.SetViewports(vp); 
                    }

                    Exception exp = null;
                    try
                    {
                        this.FLayerIn.RenderAll(context, settings);
                    }
                    catch (Exception ex)
                    {
                        exp = ex;
                    }
                    finally
                    {
                        if (enabled)
                        {
                            context.RenderTargetStack.Apply();
                        }
                    }
                    if (exp != null)
                    {
                        throw exp;
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
