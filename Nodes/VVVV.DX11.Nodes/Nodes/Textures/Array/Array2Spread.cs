
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
using VVVV.DX11.Nodes;
using VVVV.DX11;
using VVVV.DX11.Lib.Rendering;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Array2Spread", Category = "DX11.Texture2D", Version = "", Author = "sebl")]
    public class Array2Spread : IPluginEvaluate, IDX11ResourceProvider, IDisposable
    {
        [Input("TextureArray In", IsSingle = true)]
        protected Pin<DX11Resource<DX11TextureArray2D>> FTexIn;

        [Input("Width", IsSingle = true, DefaultValue = 512)]
        protected ISpread<int> Width;

        [Input("Height", IsSingle = true, DefaultValue = 512)]
        protected ISpread<int> Height;

        [Input("Depth", IsSingle = true, DefaultValue = 4)]
        protected ISpread<int> Depth;

        [Input("Format", IsSingle = true, DefaultEnumEntry = "Unknown")]
        protected ISpread<SlimDX.DXGI.Format> Format;

        [Input("Slice Index")]
        protected ISpread<int> FSliceIndex;

        [Input("Reset", IsBang = true)]
        protected ISpread<bool> FReset;

        [Input("Write", IsBang = true)]
        protected ISpread<bool> FWrite;

        [Output("Textures Out")]
        protected ISpread<DX11Resource<DX11Texture2D>> FTextureOutput;

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }


        public void Evaluate(int SpreadMax)
        {

            int ArrayCount = FTexIn.SliceCount; // no no no

            FTextureOutput.SliceCount = FTexIn.SliceCount;

            for (int i = 0; i < ArrayCount; i++)
            {
                //IDX11Resource b= this.FTexIn[this.AssignedContext];

                //this.AssignedContext.CurrentDeviceContext.CopySubresourceRegion()

                if (this.FTextureOutput[i] == null)
                {

                    this.FTextureOutput[i] = new DX11Resource<DX11Texture2D>( );
                }

                this.FTextureOutput[i] = FTexIn.
            }


        }

        


    }
}