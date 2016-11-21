using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;

namespace VVVV.DX11.Nodes.Layers
{
    [PluginInfo(Name="Validator",Category="DX11.Layer",Version="", Author="vux")]
    public class DX11LayerValidatorNode : IPluginEvaluate, IDX11LayerHost
    {
        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Validators", Order = 5001)]
        protected Pin<IDX11ObjectValidator> FInVal;

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
                List<IDX11ObjectValidator> valids = new List<IDX11ObjectValidator>();
                if (this.FInVal.IsConnected)
                {
                    for (int i = 0; i < this.FInVal.SliceCount; i++)
                    {
                        if (this.FInVal[i].Enabled)
                        {
                            IDX11ObjectValidator v = this.FInVal[i];
                            //v.Reset();
                            v.SetGlobalSettings(settings);

                            valids.Add(v);
                            settings.ObjectValidators.Add(v);
                        }
                    }
                }

                if (this.FLayerIn.IsConnected)
                {
                    this.FLayerIn.RenderAll(context, settings);
                }

                foreach (IDX11ObjectValidator v in valids)
                {
                    settings.ObjectValidators.Remove(v);
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
