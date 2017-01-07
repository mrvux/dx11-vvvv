using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using SlimDX.Direct3D11;
using SlimDX;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Enabled", Category = "DX11.Layer", Version = "", Author = "vux", Help ="Completely block update/render and evaluate if not enabled")]
    public class DX11LayerEnabledNode : IPluginEvaluate, IDX11LayerHost, IDX11UpdateBlocker, IPartImportsSatisfiedNotification
    {
        [Input("Layer In", AutoValidate = false)]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Enabled", DefaultValue = 1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        public bool Enabled
        {
            get { return this.FEnabled[0]; }
        }

        public void OnImportsSatisfied()
        {
            if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FEnabled[0])
            {
                this.FLayerIn.Sync();
            }
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
                    this.FLayerIn.RenderAll(context, settings);
                }
            }
        }
        #endregion
    }
}
