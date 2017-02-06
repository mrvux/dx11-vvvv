using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.IO;

namespace VVVV.DX11.RenderGraph.Model
{
    public class DX11NodeInterfaces
    {
        private readonly IInternalPluginHost hoster;
        private readonly IDX11RenderWindow renderWindow;
        private readonly IDX11ResourceDataRetriever dataRetriever;
        private readonly IDX11UpdateBlocker updateBlocker;

        private readonly IDX11LayerHost layerHost;
        private readonly IDX11ResourceHost resourceHost;
        private readonly IDX11RendererHost rendererHost;

        private readonly IDX11RenderStartPoint renderStartPoint;

        public DX11NodeInterfaces(IInternalPluginHost hoster)
        {
            this.hoster = hoster;
            this.renderWindow = this.IsAssignable<IDX11RenderWindow>() ? this.Instance<IDX11RenderWindow>() : null;
            
            this.dataRetriever = this.IsAssignable<IDX11ResourceDataRetriever>() ? this.Instance<IDX11ResourceDataRetriever>() : null;
            this.updateBlocker = this.IsAssignable<IDX11UpdateBlocker>() ? this.Instance<IDX11UpdateBlocker>() : null;

            this.resourceHost = this.IsAssignable<IDX11ResourceHost>() ? this.Instance<IDX11ResourceHost>() : null;
            this.rendererHost = this.IsAssignable<IDX11RendererHost>() ? this.Instance<IDX11RendererHost>() : null;
            this.layerHost = this.IsAssignable<IDX11LayerHost>() ? this.Instance<IDX11LayerHost>() : null;
            this.renderStartPoint = this.IsAssignable<IDX11RenderStartPoint>() ? this.Instance<IDX11RenderStartPoint>() : null;
        }

        public bool IsRenderStartPoint
        {
            get { return this.renderStartPoint != null; }
        }

        public bool IsResourceHost
        {
            get { return this.resourceHost != null; }
        }

        public bool IsRendererHost
        {
            get { return this.rendererHost != null; }
        }

        public bool IsLayerHost
        {
            get { return this.layerHost != null; }
        }

        public bool IsRenderWindow
        {
            get { return this.renderWindow != null; }
        }

        public bool IsDataRetriever
        {
            get { return this.dataRetriever != null; }
        }

        public bool IsUpdateBlocker
        {
            get { return this.updateBlocker != null; }
        }

        public IDX11RenderWindow RenderWindow
        {
            get { return this.renderWindow; }
        }

        public IDX11LayerHost LayerHost
        {
            get { return this.layerHost; }
        }

        public IDX11ResourceHost ResourceHost
        {
            get { return this.resourceHost; }
        }

        public IDX11RendererHost RendererHost
        {
            get { return this.rendererHost; }
        }

        public IDX11ResourceDataRetriever DataRetriever
        {
            get { return this.dataRetriever; }
        }

        public IDX11UpdateBlocker UpdateBlocker
        {
            get { return this.updateBlocker; }
        }


        public IDX11RenderStartPoint RenderStartPoint
        {
            get { return this.renderStartPoint; }
        }

        private T Instance<T>()
        {
            IInternalPluginHost iip = (IInternalPluginHost)this.hoster;

            if (iip.Plugin is PluginContainer)
            {
                PluginContainer plugin = (PluginContainer)iip.Plugin;
                return (T)plugin.PluginBase;
            }
            else
            {
                return (T)iip.Plugin;
            }
        }

        private bool IsAssignable<T>()
        {
            IInternalPluginHost iip = (IInternalPluginHost)this.hoster;

            if (iip.Plugin is PluginContainer)
            {
                PluginContainer plugin = (PluginContainer)iip.Plugin;
                return typeof(T).IsAssignableFrom(plugin.PluginBase.GetType());
            }
            else
            {
                return typeof(T).IsAssignableFrom(iip.Plugin.GetType());
            }

        }
    }
}
