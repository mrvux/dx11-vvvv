using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using SlimDX.Direct3D11;
using VVVV.DX11.Lib.Rendering;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "ResetCounter", Category = "DX11.Layer", Version = "Advanced", Author = "microdee")]
    public class DX11AdvancedResetCounterNode : IPluginEvaluate, IDX11LayerProvider
    {
        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("RWStructuredBuffer Semantic")]
        protected ISpread<string> FSemantic;

        [Input("Reset Counter")]
        protected ISpread<bool> FInReset;

        [Input("Counter Value")]
        protected ISpread<int> FInResetCounterValue;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }
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
            if (this.FLayerIn.IsConnected)
            {
                for (int i = 0; i < FSemantic.SliceCount; i++)
                {
                    RWStructuredBufferRenderSemantic ccrs = null;
                    foreach (var rsem in settings.CustomSemantics)
                    {
                        if (rsem.Semantic == FSemantic[i])
                        {
                            if (rsem is RWStructuredBufferRenderSemantic)
                            {
                                ccrs = rsem as RWStructuredBufferRenderSemantic;
                            }
                        }
                    }

                    if (FInReset[i])
                    {
                        if (ccrs != null)
                        {
                            int[] resetval = { FInResetCounterValue[i] };
                            var uavarray = new UnorderedAccessView[1] { ccrs.Data.UAV };
                            context.CurrentDeviceContext.ComputeShader.SetUnorderedAccessViews(uavarray, 0, 1, resetval);
                        }
                    }
                }
                this.FLayerIn[0][context].Render(this.FLayerIn.PluginIO, context, settings);
            }
        }
        #endregion
    }
}
