using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;



namespace VVVV.DX11.Nodes
{
    public abstract class AbstractDX11LayerSpaceNode : IPluginEvaluate, IDX11LayerProvider, IDX11UpdateBlocker
    {
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
            if (this.FLayerIn.SliceCount == 0) { return; }

            if (this.FEnabled[0])
            {
                if (this.FLayerIn.PluginIO.IsConnected)
                {
                    Matrix view = settings.View;
                    Matrix projection = settings.Projection;
                    Matrix vp = settings.ViewProjection;
                    bool depthonly = settings.DepthOnly;

                    this.UpdateSettings(settings);

                    this.FLayerIn[0][context].Render(this.FLayerIn.PluginIO, context, settings);

                    settings.View = view;
                    settings.Projection = projection;
                    settings.ViewProjection = vp;
                    settings.DepthOnly = depthonly;
                }
            }
            else
            {
                this.FLayerIn[0][context].Render(this.FLayerIn.PluginIO, context, settings);
            }
        }

        protected abstract void UpdateSettings(DX11RenderSettings settings);


        #endregion

        public bool Enabled
        {
            get { return this.FEnabled[0]; }
        }
    }




}
