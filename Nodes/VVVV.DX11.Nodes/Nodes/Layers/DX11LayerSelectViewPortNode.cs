using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name="SelectViewport",Category="DX11.Layer",Version="", Author="vux")]
    public class DX11LayerSelectViewportNode : IPluginEvaluate, IDX11LayerHost
    {
        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Viewport Index", DefaultValue = -1)]
        protected ISpread<int> FViewPortIndex;

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
            if (this.FLayerIn.IsConnected)
            {
                bool allow = false;
                for (int i = 0; i < this.FViewPortIndex.SliceCount;i++)
                {
                    if (this.FViewPortIndex[i] < 0)
                    {
                        allow = true;
                    }
                    else if (this.FViewPortIndex[i] % settings.ViewportCount == settings.ViewportIndex)
                    {
                        allow = true;
                    }
                }

                if (allow)
                {
                    if (this.FLayerIn.IsConnected)
                    {
                        this.FLayerIn.RenderAll(context, settings);
                    }
                }
            }

        }

        #endregion
    }
}
