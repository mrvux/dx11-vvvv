
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Composition;


using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;
using VVVV.DX11.Nodes;
using VVVV.DX11;
using VVVV.DX11.Lib.Rendering;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Array2Spread", Category = "DX11.Texture2D", Version = "", Author = "sebl")]
    public class Array2Spread : IPluginEvaluate, IDX11ResourceProvider, IDisposable
    {
        [Input("TextureArray In", IsSingle = true)]
        //protected Pin<DX11Resource<DX11TextureArray2D>> FTexIn;
        protected Pin<DX11Resource<DX11RenderTextureArray>> FTexIn;

        [Input("Element Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInElementCount;

        //[Input("Reset", IsBang = true)]
        //protected ISpread<bool> FReset;

        [Output("Textures Out")]
        protected ISpread<DX11Resource<DX11Texture2D>> FTextureOutput;



        public void Evaluate(int SpreadMax)
        {
            FTextureOutput.SliceCount = FInElementCount[0];

            for (int i = 0; i < FInElementCount[0]; i++)
            {
                if (this.FTextureOutput[i] == null)
                {
                    this.FTextureOutput[i] = new DX11Resource<DX11Texture2D>();
                }
               
            }

        }
        
        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (this.FTextureOutput.SliceCount == 0) { return; }

            int ArrayCount = FTexIn[0][context].ElemCnt;

            for (int i = 0; i < ArrayCount; i++)
            {
                //if (!this.FTextureOutput[i].Contains(context))
                //{
                Texture2DDescription desc;

                    if (this.FTextureOutput[i].Contains(context))
                    {
                        desc = FTexIn[0][context].Resource.Description;

                        if (desc.Width != this.FTexIn[0][context].Resource.Description.Width || desc.Height != this.FTexIn[0][context].Resource.Description.Height || desc.Format != this.FTexIn[0][context].Resource.Description.Format)
                        {
                            this.FTextureOutput[i].Dispose(context);
                            this.FTextureOutput[i] = new DX11Resource<DX11Texture2D>();
                        }
                    }
                    else
                    {
                        this.FTextureOutput[i][context] = new DX11Texture2D();
                    }

                    //desc = this.FTextureOutput[i][context].Resource.Description;

                    desc = this.FTexIn[0][context].Resource.Description;

                    SlimDX.Direct3D11.Resource source = this.FTexIn[0][context].Resource;
                    SlimDX.Direct3D11.Resource destiantion = this.FTextureOutput[i][context].Resource;

                    // here's the error
                    context.CurrentDeviceContext.CopySubresourceRegion(source, i, destiantion, 0, 0, 0, 0);

                //}
                //else // contains context
                //{
                //    IDX11ResourceProvider provider = source.Instance<IDX11ResourceProvider>();
                //    this.FTextureOutput[i].Assign(context.);
                //}
            }
        }
        

        public void Dispose()
        {
            if (this.FTextureOutput.SliceCount > 0)
            {
                if (this.FTextureOutput[0] != null)
                {
                    this.FTextureOutput[0].Dispose();
                }
            }
        }


        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            for (int i = 0; i < FTextureOutput.SliceCount; i++ )
            {
                this.FTextureOutput[i].Dispose(context);
            }
        }


    }
}