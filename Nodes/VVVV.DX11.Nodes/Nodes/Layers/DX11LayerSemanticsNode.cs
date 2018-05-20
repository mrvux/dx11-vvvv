using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name="Semantics",Category="DX11.Layer",Version="", Author="vux")]
    public class DX11LayerSemanticsNode : IPluginEvaluate, IDX11LayerHost
    {
        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Custom Semantics", Order = 5000)]
        protected Pin<IDX11RenderSemantic> FInSemantics;

        [Input("Resource Semantics", Order = 5001)]
        protected Pin<DX11Resource<IDX11RenderSemantic>> FInResSemantics;

        [Input("Enabled",DefaultValue=1, Order = 100000)]
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
            if (this.FEnabled[0])
            {
                if (this.FLayerIn.IsConnected)
                {
                    List<IDX11RenderSemantic> semantics = new List<IDX11RenderSemantic>();
                    if (this.FInSemantics.IsConnected)
                    {
                        semantics.AddRange(this.FInSemantics);
                        settings.CustomSemantics.AddRange(semantics);
                    }


                    List<DX11Resource<IDX11RenderSemantic>> ressemantics = new List<DX11Resource<IDX11RenderSemantic>>();
                    if (this.FInResSemantics.IsConnected)
                    {
                        ressemantics.AddRange(this.FInResSemantics);
                        settings.ResourceSemantics.AddRange(ressemantics);
                    }

                    this.FLayerIn.RenderAll(context, settings);

                    foreach (IDX11RenderSemantic semantic in semantics)
                    {
                        settings.CustomSemantics.Remove(semantic);
                    }

                    foreach (DX11Resource<IDX11RenderSemantic> rs in ressemantics)
                    {
                        settings.ResourceSemantics.Remove(rs);
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
