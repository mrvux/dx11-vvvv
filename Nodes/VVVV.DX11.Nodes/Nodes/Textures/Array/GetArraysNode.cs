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
    [PluginInfo(Name = "GetArray", Category = "DX11.TextureArray", Version = "BinSize", Author = "sebl")]
    public class GetArraysNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("TextureArray In", IsSingle = true)]
        protected Pin<DX11Resource<DX11RenderTextureArray>> FTexIn;

        [Input("Index")]
        protected ISpread<ISpread<int>> FIndex;

        [Output("TextureArray Out")]
        protected ISpread<DX11Resource<DX11RenderTextureArray>> FTextureOutput;

        private Spread<DX11Resource<CopySubArray>> generators = new Spread<DX11Resource<CopySubArray>>();
        private int binSize;


        public void Evaluate(int SpreadMax)
        {
            if (this.FTexIn.IsConnected)
            {
                binSize = FIndex.SliceCount;

                // manage generators
                if (generators.SliceCount > binSize)
                {
                    for (int i = generators.SliceCount; i > binSize; i--)
                    {
                        generators[i].Dispose();
                    }
                }

                if (generators.SliceCount < binSize)
                {
                    for (int i = generators.SliceCount; i < binSize; i++)
                    {
                        generators.Add(new DX11Resource<CopySubArray>());
                    }
                }


                // manage TextureOutput(s)
                if (this.FTextureOutput.SliceCount > binSize)
                {
                    for (int t = binSize; t < this.FTextureOutput.SliceCount; t++)
                    {
                        if (this.FTextureOutput[t] != null)
                            this.FTextureOutput[t].Dispose();

                    }
                }

                FTextureOutput.SliceCount = binSize;

                for (int i = 0; i < binSize; i++)
                {
                    if (this.FTextureOutput[i] == null)
                    {
                        this.FTextureOutput[i] = new DX11Resource<DX11RenderTextureArray>();
                    }
                }

            }
            else
            {
                for (int i = 0; i < FTextureOutput.SliceCount; i++)
                {
                    if (this.FTextureOutput[i] != null)
                        this.FTextureOutput[i].Dispose();
                }
                FTextureOutput.SliceCount = 0;
            }
        }


        public void Update(DX11RenderContext context)
        {
            if (this.FTexIn.IsConnected)
            {
                for (int i = 0; i < binSize; i++)
                {
                    if (!this.generators[i].Contains(context))
                    {
                        this.generators[i][context] = new CopySubArray(context);
                    }

                    var generator = this.generators[i][context];

                    generator.Apply(this.FTexIn[0], this.FIndex[i]);
                    this.WriteResult(generator, context, i);
                }
            }
        }


        private void WriteResult(CopySubArray generator, DX11RenderContext context, int slice)
        {
            DX11RenderTextureArray result = generator.Result;
            this.FTextureOutput[slice][context] = generator.Result;
        }


        public void Destroy(DX11RenderContext context, bool force)
        {
            foreach (var g in generators)
            {
                if (g != null && g.Contains(context))
                {
                    g.Dispose(context);
                }
            }
        }


        public void Dispose()
        {
            foreach (var g in this.generators)
            {
                if (g != null)
                {
                    g.Dispose();
                }
            }
            this.generators = null;
        }

    }
}