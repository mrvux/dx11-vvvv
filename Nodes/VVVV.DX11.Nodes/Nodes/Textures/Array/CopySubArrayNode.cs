using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Composition;


using SlimDX;
using SlimDX.Direct3D11;
//using SlimDX.DXGI;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;
using VVVV.DX11.Nodes;
using VVVV.DX11;
using VVVV.DX11.Lib.Rendering;

using VVVV.Core.Logging;
using VVVV.Utils.VMath;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "GetArray", Category = "DX11.TextureArray", Version = "", Author = "sebl")]
    public class CopySubArrayNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("TextureArray In", IsSingle = true)]
        protected Pin<DX11Resource<DX11RenderTextureArray>> FTexIn;

        [Input("Index")]
        protected IDiffSpread<int> FIndex;

        [Output("Textures Out")]
        protected ISpread<DX11Resource<DX11RenderTextureArray>> FTextureOutput;

        private DX11Resource<CopySubArray> generators = new DX11Resource<CopySubArray>();


        public void Evaluate(int SpreadMax)
        {
            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11RenderTextureArray>();
            }
        }


        public void Update(DX11RenderContext context)
        {
            if (!this.generators.Contains(context))
            {
                this.generators[context] = new CopySubArray(context);
            }

            if (this.FTexIn.IsConnected)
            {
                var generator = this.generators[context];

                generator.Apply(this.FTexIn[0], this.FIndex);
                this.WriteResult(generator, context);
            }
        }


        private void WriteResult(CopySubArray generator, DX11RenderContext context)
        {
            DX11RenderTextureArray result = generator.Result;
            this.FTextureOutput[0][context] = generator.Result;
        }


        public void Destroy(DX11RenderContext context, bool force)
        {
            if (this.generators != null && this.generators.Contains(context))
            {
                this.generators.Dispose(context);
            }
        }


        public void Dispose()
        {
            if (this.generators != null)
            {
                this.generators.Dispose();
                this.generators = null;
            }
        }

    }
}