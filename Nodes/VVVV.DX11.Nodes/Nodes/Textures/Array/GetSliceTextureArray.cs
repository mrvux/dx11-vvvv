
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
    [PluginInfo(Name = "GetSlice", Category = "DX11.TextureArray", Version = "", Author = "sebl")]
    public class GetSliceTextureArray : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("TextureArray In", IsSingle = true)]
        protected Pin<DX11Resource<DX11RenderTextureArray>> FTexIn;

        [Input("Index")]
        protected IDiffSpread<int> FIndex;

        [Output("Textures Out")]
        protected ISpread<DX11Resource<DX11Texture2D>> FTextureOutput;

        int ArrayCount = 1;
        int numSlicesOut;

        [Import()]
        public ILogger logger;

        public void Evaluate(int SpreadMax)
        {
            if (this.FTexIn.IsConnected)
            {
                //FTextureOutput.SliceCount = ArrayCount;
                this.numSlicesOut = this.FIndex.SliceCount;
                this.FTextureOutput.SliceCount = this.numSlicesOut;

                for (int i = 0; i < numSlicesOut; i++)
                {
                    if (this.FTextureOutput[i] == null)
                    {
                        this.FTextureOutput[i] = new DX11Resource<DX11Texture2D>();
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
                this.ArrayCount = 1;
            }

            this.FTextureOutput.SliceCount = this.numSlicesOut;

            if (this.FTextureOutput.SliceCount > this.numSlicesOut)
            {
                for (int t = numSlicesOut; t < this.FTextureOutput.SliceCount; t++)
                {
                    this.FTextureOutput[t].Dispose();
                }
            }
        }

        
        public void Update(DX11RenderContext context)
        {
            if (this.FTextureOutput.SliceCount == 0 || !FTexIn.IsConnected || !FTexIn[0].Contains(context)) { return; }

            if (FTexIn.IsConnected)
            {
                ArrayCount = FTexIn[0][context].ElemCnt;

                for (int i = 0; i < numSlicesOut; i++)
                {
                    int slice = VMath.Zmod(FIndex[i], ArrayCount);

                    Texture2DDescription descIn;
                    Texture2DDescription descOut;

                    if (this.FTextureOutput[i].Contains(context))
                    {
                        descIn = FTexIn[0][context].Resource.Description;
                        descOut = this.FTextureOutput[i][context].Resource.Description;

                        if (/*FIndex.IsChanged ||*/ descIn.Format != descOut.Format || descIn.Width != descOut.Width || descIn.Height != descOut.Height)
                        {
                            //this.logger.Log(LogType.Message, "init slice " + i);
                            InitTexture(context, i, descIn);                           
                        }
                    }
                    else
                    {
                        //InitTexture(context, i, descIn);  
                        this.FTextureOutput[i][context] = new DX11Texture2D();
                    }

                    descIn = this.FTexIn[0][context].Resource.Description;

                    if (this.FTextureOutput[i][context].Resource == null)
                    {
                        //this.logger.Log(LogType.Message, "init slice " + i);
                        InitTexture(context, i, descIn);
                    }

                    SlimDX.Direct3D11.Resource source = this.FTexIn[0][context].Resource;
                    SlimDX.Direct3D11.Resource destination = this.FTextureOutput[i][context].Resource;

                    int sourceSubres = SlimDX.Direct3D11.Texture2D.CalculateSubresourceIndex(0, slice, descIn.MipLevels);
                    int destinationSubres = SlimDX.Direct3D11.Texture2D.CalculateSubresourceIndex(0, 0, 1);

                    //this.logger.Log(LogType.Message, "get slice " + slice + " into " + i);
                    context.CurrentDeviceContext.CopySubresourceRegion(source, sourceSubres, destination, destinationSubres, 0, 0, 0);
                }
            }
        }

        private void InitTexture(DX11RenderContext context, int index, Texture2DDescription description)
        {
            this.FTextureOutput[index].Dispose(context);
            this.FTextureOutput[index] = new DX11Resource<DX11Texture2D>();
            description.ArraySize = 1;
            this.FTextureOutput[index][context] = DX11Texture2D.FromDescription(context, description);
        }
        

        public void Dispose()
        {
            this.FTextureOutput.SafeDisposeAll();
        }


        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FTextureOutput.SafeDisposeAll(context);
        }


    }
}