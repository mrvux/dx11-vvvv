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
    [PluginInfo(Name="Tag",Category="DX11.Layer",Version="", Author="vux")]
    public class DX11LayerTagNode : IPluginEvaluate, IDX11LayerHost
    {
        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Tag", IsSingle = true)]
        protected ISpread<object> tag;

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
                    var currentTag = settings.Tag;

                    settings.Tag = tag.SliceCount > 0 ? tag[0] : null;

                    for (int i = 0; i < this.FLayerIn.SliceCount; i++)
                    {
                        this.FLayerIn[i][context].Render(context, settings);
                    }

                    settings.Tag = currentTag;

                }
            }
        }

        #endregion
    }
}
