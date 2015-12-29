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
    [PluginInfo(Name = "SetSlice", Category = "DX11.Texture2D", Version = "", Author="vux")]
    public class TextureArraySetSliceNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Texture In", IsSingle = true)]
        protected Pin<DX11Resource<DX11Texture2D>> FTexIn;

        [Input("Width", IsSingle = true, DefaultValue=512)]
        protected ISpread<int> Width;

        [Input("Height", IsSingle = true,DefaultValue=512)]
        protected ISpread<int> Height;

        [Input("Depth", IsSingle = true, DefaultValue = 4)]
        protected ISpread<int> Depth;

        [Input("Format", IsSingle = true, DefaultEnumEntry="Unknown")]
        protected ISpread<SlimDX.DXGI.Format> Format;

        [Input("Slice Index")]
        protected ISpread<int> FSliceIndex;

        [Input("Reset", IsBang = true)]
        protected ISpread<bool> FReset;

        [Input("Write", IsBang = true)]
        protected ISpread<bool> FWrite;

        [Output("Texture Array", IsSingle = true)]
        protected ISpread<DX11Resource<DX11RenderTextureArray>> FOutTB;

        private TextureArraySetSlice generator;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutTB[0] == null)
            {
                this.FOutTB[0] = new DX11Resource<DX11RenderTextureArray>();
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (this.generator != null) { this.generator.Dispose(); this.generator = null; }
        }

        public void Update(DX11RenderContext context)
        {
            if (generator == null) { generator = new TextureArraySetSlice(context); }

            if (this.FTexIn.PluginIO.IsConnected)
            {
                if (this.FReset[0])
                {
                    generator.Reset(this.FTexIn[0][context], this.Width[0], this.Height[0], this.Depth[0], this.Format[0]);
                    this.FOutTB[0][context] = generator.Result;
                }
                else if (this.FWrite[0])
                {
                    generator.Apply(this.FTexIn[0][context], this.Width[0], this.Height[0], this.Depth[0], this.Format[0], this.FSliceIndex[0]);
                    this.FOutTB[0][context] = generator.Result;
                }
            }
        }

        public void Dispose()
        {
            if (this.generator != null) { this.generator.Dispose(); }
        }
    }
}
