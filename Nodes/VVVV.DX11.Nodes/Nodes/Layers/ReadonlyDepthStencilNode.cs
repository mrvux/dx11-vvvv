using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using VVVV.Core.Logging;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "ReadonlyDepth", Category = "DX11.Layer", Version = "", Author = "vux")]
    public class DX11ReadonlyDepthNode : IPluginEvaluate, IDX11LayerHost
    {
        [Import()]
        protected ILogger logger;

        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Enabled", DefaultValue = 1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutLayer[0] == null)
            {
                this.FOutLayer[0] = new DX11Resource<DX11Layer>();
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
                    RenderTargetStackElement currentFrameBuffer = context.RenderTargetStack.Current;

                    if (currentFrameBuffer.DepthStencil != null && currentFrameBuffer.DepthStencil is DX11DepthStencil)
                    {
                        context.RenderTargetStack.Push(currentFrameBuffer.DepthStencil, true, currentFrameBuffer.RenderTargets);

                        this.FLayerIn.RenderAll(context, settings);

                        context.RenderTargetStack.Pop();
                    }
                    else
                    {
                        logger.Log(LogType.Warning, "Trying to attach a depth stencil as readonly, but either none is bound or option is not available");
                        this.FLayerIn.RenderAll(context, settings);
                    }
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
