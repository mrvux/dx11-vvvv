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
    [PluginInfo(Name = "ResetCounter", Category = "DX11.Layer", Version = "", Author = "vux")]
    public class DX11ResetCounterNode : IPluginEvaluate, IDX11LayerHost
    {
        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Reset Counter", IsSingle = true, DefaultValue = 1)]
        protected Pin<bool> FInReset;

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
            this.FOutLayer.SafeDisposeAll(context);
        }

        public void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.FLayerIn.IsConnected)
            {
                bool current = settings.ResetCounter;
                settings.ResetCounter = this.FInReset[0];

                for (int i = 0; i < this.FLayerIn.SliceCount; i++)
                {
                    this.FLayerIn[i][context].Render(context, settings);
                }

                settings.ResetCounter = current;
            }
        }
        #endregion
    }
}
