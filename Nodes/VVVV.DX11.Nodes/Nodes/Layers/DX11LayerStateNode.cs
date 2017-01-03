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
    [PluginInfo(Name="RenderState",Category="DX11.Layer",Version="", Author="vux")]
    public class DX11LayerStateNode : IPluginEvaluate, IDX11LayerHost
    {
        [Input("Render State", IsSingle = true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

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
                if (this.FLayerIn.IsConnected)
                {
                    bool popstate = false;

                    if (this.FInState.IsConnected)
                    {
                        context.RenderStateStack.Push(this.FInState[0]);
                        popstate = true;
                    }

                    for (int i = 0; i < this.FLayerIn.SliceCount; i++)
                    {
                        this.FLayerIn[i][context].Render(context, settings);
                    }

                    if (popstate) { context.RenderStateStack.Pop(); }

                }
            }
            else
            {
                if (this.FLayerIn.IsConnected)
                {
                    for (int i = 0; i < this.FLayerIn.SliceCount; i++)
                    {
                        this.FLayerIn[i][context].Render(context, settings);
                    }
                }
            }
        }

        #endregion

    }
}
