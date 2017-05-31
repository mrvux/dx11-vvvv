using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D11;


namespace VVVV.DX11.Nodes.Textures
{
    [PluginInfo(Name = "FromSharedTexture", Category = "DX11.Texture", Version = "2d", Author = "vux")]
    public class PointerTextureNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Pointer", AsInt=true)]
        protected IDiffSpread<uint> FPointer;

        [Output("Texture")]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureOutput;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        protected bool FInvalidate;

        public void Evaluate(int SpreadMax)
        {
            if (this.FPointer.SliceCount == 0)
            {
                this.FTextureOutput.SafeDisposeAll();
                this.FTextureOutput.SliceCount = 0;
                return;
            }

            this.FValid.SliceCount = SpreadMax;
            this.FTextureOutput.SliceCount = SpreadMax;


            if (this.FPointer.IsChanged)
            {
                this.FInvalidate = true;
                this.FTextureOutput.SafeDisposeAll();
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FTextureOutput[i] == null)
                {
                    FTextureOutput[i] = new DX11Resource<DX11Texture2D>();
                }
            }

        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInvalidate)
            {
                for (int i = 0; i < FTextureOutput.SliceCount; i++)
                {
                    try
                    {
                        int p = unchecked((int) this.FPointer[i]);
                        IntPtr share = new IntPtr(p);
                        DX11Texture2D resource = DX11Texture2D.FromSharedHandle(context, share);
                        this.FTextureOutput[i][context] = resource;
                        this.FValid[i] = true;
                    }
                    catch (Exception)
                    {
                        this.FValid[i] = false;
                    }
                }
                this.FInvalidate = false;
            }           
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FTextureOutput.SafeDisposeAll(context);
        }

        public void Dispose()
        {
            this.FTextureOutput.SafeDisposeAll();
        }
    }
}
