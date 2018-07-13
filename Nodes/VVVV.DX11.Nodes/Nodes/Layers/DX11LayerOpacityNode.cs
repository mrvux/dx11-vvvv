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
    [PluginInfo(Name="Opacity",Category="DX11.Layer",Version="", Author="vux")]
    public class DX11LayerOpacityNode : IPluginEvaluate, IDX11LayerHost
    {
        public enum OpacityOperation
        {
            Replace = 0,
            Multiply = 1,
            Add = 2,
        }

        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Opacity", DefaultValue =1.0f, IsSingle = true)]
        protected ISpread<float> opacity;

        [Input("Operation", IsSingle = true)]
        protected ISpread<OpacityOperation> operation;

        [Input("Disable Render On Zero Opacity", IsSingle = true, DefaultValue =0)]
        protected ISpread<bool> disableIfZero;

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
            this.FOutLayer.SafeDisposeAll(context);
        }

        public void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.FEnabled[0])
            {
                if (this.FLayerIn.IsConnected && this.opacity.SliceCount > 0 && this.disableIfZero.SliceCount > 0)
                {
                    if (this.disableIfZero[0])
                    {
                        if (this.opacity[0] <= 0.0f)
                            return;
                    }

                    var current = settings.LayerOpacity;

                    switch(this.operation[0])
                    {
                        case OpacityOperation.Replace:
                            settings.LayerOpacity = this.opacity[0];
                            break;
                        case OpacityOperation.Multiply:
                            settings.LayerOpacity *= this.opacity[0];
                            break;
                        case OpacityOperation.Add:
                            settings.LayerOpacity += this.opacity[0];
                            break;
                    }

                   

                    this.FLayerIn.RenderAll(context, settings);

                    settings.LayerOpacity = current;

                }
            }
        }

        #endregion
    }
}
