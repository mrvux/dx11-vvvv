using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Composition;


using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    public abstract class BaseComputeRenderer : IPluginEvaluate, IDX11RendererProvider, IDisposable, IPartImportsSatisfiedNotification
    {
        [Import()]
        protected IIOFactory iofactory;

        [Input("Layer In", IsSingle = true)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Enabled", DefaultValue = 1, Order = 5000)]
        protected ISpread<bool> FInEnabled;




        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();

        private Dictionary<DX11RenderContext, DX11RenderSettings> settings = new Dictionary<DX11RenderContext, DX11RenderSettings>();

        protected abstract void OnEvaluate(int SpreadMax);
        protected abstract void OnUpdate(DX11RenderContext context, DX11RenderSettings settings);
        protected abstract void OnDestroy(DX11RenderContext context);

        protected virtual void PreRender(DX11RenderContext context, DX11RenderSettings settings) { }
        protected virtual void PostRender(DX11RenderContext context) { }
        protected virtual bool AllowEmptyLayer { get { return false; } }

        public abstract void Dispose();

        protected void InvalidateSettings() { this.settings.Clear(); }

        public void Evaluate(int SpreadMax)
        {
            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            this.OnEvaluate(SpreadMax);
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (!this.settings.ContainsKey(context))
            {
                DX11RenderSettings rs = new DX11RenderSettings();
                this.settings.Add(context, rs);

                this.OnUpdate(context, rs);
            }

            this.updateddevices.Add(context);
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            if (this.settings.ContainsKey(context))
            {
                this.OnDestroy(context);
                this.settings.Remove(context);
            }
        }

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }

        public void Render(DX11RenderContext context)
        {
            if (!this.FInLayer.PluginIO.IsConnected && this.AllowEmptyLayer == false) { return; }

            if (this.rendereddevices.Contains(context)) { return; }

            if (!this.updateddevices.Contains(context))
            {
                this.Update(null, context);
            }

            if (this.FInEnabled[0])
            {
                DX11RenderSettings rs = this.settings[context];

                this.PreRender(context, rs);

                for (int j = 0; j < this.FInLayer.SliceCount; j++)
                {
                    this.FInLayer[j][context].Render(this.FInLayer.PluginIO, context, rs);
                }

                this.PostRender(context);
            }
        }

        public void OnImportsSatisfied()
        {

        }
    }
}
