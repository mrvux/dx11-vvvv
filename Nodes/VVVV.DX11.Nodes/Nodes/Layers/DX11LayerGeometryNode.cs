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
    [PluginInfo(Name="Geometry",Category="DX11.Layer",Version="", Author="vux")]
    public class DX11LayerGeometryNode : IPluginEvaluate, IDX11LayerProvider, IDX11UpdateBlocker
    {
        [Input("Geometry In", IsSingle = true)]
        protected Pin<DX11Resource<IDX11Geometry>> FInGeometry;

        [Input("Layer In", AutoValidate = false)]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Enabled",DefaultValue=1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }

            if (this.FEnabled[0])
            {
                this.FLayerIn.Sync();
            }
        }


        #region IDX11ResourceProvider Members

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (!this.FOutLayer[0].Contains(context))
            {
                this.FOutLayer[0][context] = new DX11Layer();
                this.FOutLayer[0][context].Render = this.Render;
            }
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            this.FOutLayer[0].Dispose(context);
        }

        public void Render(IPluginIO pin, DX11RenderContext context, DX11RenderSettings settings)
        {
            IDX11Geometry g = settings.Geometry;
            if (this.FEnabled[0])
            {
                if (this.FLayerIn.PluginIO.IsConnected)
                {
                    if (this.FInGeometry.PluginIO.IsConnected)
                    {
                        settings.Geometry = this.FInGeometry[0][context];
                    }

                    this.FLayerIn[0][context].Render(this.FLayerIn.PluginIO, context, settings);
                }
            }
            settings.Geometry = g;
        }

        #endregion

        public bool Enabled
        {
            get { return this.FEnabled[0]; }
        }
    }
}
