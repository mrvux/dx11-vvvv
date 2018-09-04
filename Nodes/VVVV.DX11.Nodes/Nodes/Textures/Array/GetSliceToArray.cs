
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
    [PluginInfo(Name = "GetSlice", Category = "DX11.TextureArray", Version = "To Array", Author = "sebl")]
    public class GetSliceToArray : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> FTexIn;

        [Input("Max MipLevels", DefaultValue = -1, Visibility = PinVisibility.Hidden)]
        protected ISpread<int> FMaxMipLevels;

        [Input("Index", DefaultValue = -1)]
        protected ISpread<ISpread<int>> FIndex;

        [Output("Textures Out")]
        protected ISpread<DX11Resource<DX11RenderTextureArray>> FTextureOutput;

        int numSlicesOut;

        [Import()]
        public ILogger logger;

        public void Evaluate(int SpreadMax)
        {
            if (this.FTexIn.IsConnected && this.FIndex[0].SliceCount != 0)
            {
                if (this.FIndex[0][0] == -1)
                    this.numSlicesOut = 1;
                else
                    this.numSlicesOut = this.FIndex.SliceCount;

                this.FTextureOutput.SliceCount = this.numSlicesOut;

                for (int i = 0; i < numSlicesOut; i++)
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
            }

            this.FTextureOutput.SliceCount = this.numSlicesOut; // hmmmm...

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

            if (FTexIn.IsConnected && this.FIndex[0].SliceCount != 0)
            {
                int mips = 1;

                // first texture determines description; all input textures have to match w,h,d,f, mips, etc.
                Texture2DDescription descIn = FTexIn[0][context].Resource.Description; 
                Texture2DDescription descOut;

                for (int i = 0; i < numSlicesOut; i++) // for each bin
                {
                    if (FMaxMipLevels[i] <= 0)
                    {
                        mips = descIn.MipLevels;
                    }
                    else
                    {
                        mips = (int)Utils.VMath.VMath.Min(descIn.MipLevels, FMaxMipLevels[i]);
                    }

                    int currentArraySize;
                    if (FIndex[0][0] == -1)
                        currentArraySize = FTexIn.SliceCount;
                    else
                        currentArraySize = FIndex[i].SliceCount;

                    for (int j = 0; j < currentArraySize; j++) // for each slice in that bin
                    {
                        int currentslice = FIndex[i][j];

                        if (this.FTextureOutput[i].Contains(context))
                        {

                            descOut = this.FTextureOutput[i][context].Resource.Description;

                            if (/*FIndex.IsChanged ||*/ 
                                descIn.Format != descOut.Format || 
                                descIn.Width != descOut.Width || 
                                descIn.Height != descOut.Height ||
                                descOut.MipLevels != mips )
                            {
                                this.FTextureOutput[i][context] = new DX11RenderTextureArray(context, descIn.Width, descIn.Height, currentArraySize, descIn.Format, true, mips);
                            }
                        }
                        else
                        {
                            this.FTextureOutput[i][context] = new DX11RenderTextureArray(context, descIn.Width, descIn.Height, currentArraySize, descIn.Format, true, mips);
                        }

                        if (this.FTextureOutput[i][context].Resource == null)
                        {
                            this.FTextureOutput[i][context] = new DX11RenderTextureArray(context, descIn.Width, descIn.Height, currentArraySize, descIn.Format, true, mips);
                        }

                        SlimDX.Direct3D11.Resource source = this.FTexIn[currentslice][context].Resource;
                        SlimDX.Direct3D11.Resource destination = this.FTextureOutput[i][context].Resource;

                        descOut = this.FTextureOutput[i][context].Resource.Description;
                      
                        for (int m = 0; m < mips; m++)
                        {
                            int sourceSubres = SlimDX.Direct3D11.Texture2D.CalculateSubresourceIndex(m, 0, descIn.MipLevels);
                            int destinationSubres = SlimDX.Direct3D11.Texture2D.CalculateSubresourceIndex(m, j, mips);

                            context.CurrentDeviceContext.CopySubresourceRegion(source, sourceSubres, destination, destinationSubres, 0, 0, 0);
                        }
                    }
                }
            }
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