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
    [PluginInfo(Name="Spread",Category="DX11.Layer",Version="", Author="vux")]
    public class DX11LayerSpreadNode : IPluginEvaluate, IDX11LayerHost
    {
        private class SliceRenderer : IDX11LayerOrder
        {
            private List<int> slice = new List<int>();
            private Pin<DX11Resource<DX11Layer>> FLayerIn;

            public SliceRenderer(int slice, Pin<DX11Resource<DX11Layer>> layer)
            {
                this.slice.Add(slice);
                this.FLayerIn = layer;
            }

            public void Render(DX11RenderContext context, DX11RenderSettings settings)
            {
                IDX11LayerOrder currentOrder = settings.LayerOrder;
                settings.LayerOrder = this;
                if (this.FLayerIn.IsConnected)
                {
                    for (int i = 0; i < this.FLayerIn.SliceCount; i++)
                    {
                        this.FLayerIn[i][context].Render(context, settings);
                    }
                }
                settings.LayerOrder = currentOrder;
            }

            public bool Enabled
            {
                get { return true; }
            }

            public List<int> Reorder(DX11RenderSettings settings, List<DX11ObjectRenderSettings> objectSettings)
            {
                return this.slice;
            }
        }

        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Layer Count", Order = 5001, IsSingle=true, DefaultValue=1)]
        protected IDiffSpread<int> FInVal;

        [Input("Enabled",DefaultValue=1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        public void Evaluate(int SpreadMax)
        {
            if (this.FEnabled[0])
            {
                this.FOutLayer.SliceCount = this.FInVal[0];

                for (int i = 0; i < this.FOutLayer.SliceCount;i++ )
                {
                    if (this.FOutLayer[i] == null) { this.FOutLayer[i] = new DX11Resource<DX11Layer>(); }
                }
            }
            else
            {
                this.FOutLayer.SliceCount = 1;
                if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }
            }
        }


        #region IDX11ResourceProvider Members

        public void Update(DX11RenderContext context)
        {
            if (this.FEnabled[0])
            {
                for (int i = 0; i < this.FOutLayer.SliceCount; i++)
                {
                    this.FOutLayer[i][context] = new DX11Layer();
                    SliceRenderer slice = new SliceRenderer(i, this.FLayerIn);
                    this.FOutLayer[i][context].Render = slice.Render;
                }
            }
            else
            {
                if (!this.FOutLayer[0].Contains(context))
                {
                    this.FOutLayer[0][context] = new DX11Layer();
                    this.FOutLayer[0][context].Render = this.Render;
                }
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
                for (int i = 0; i < this.FLayerIn.SliceCount; i++)
                {
                    this.FLayerIn[i][context].Render(context, settings);
                }
            }
        }

        #endregion

        public bool Enabled
        {
            get { return this.FEnabled[0]; }
        }
    }
}
