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
    [PluginInfo(Name = "FromSharedTexture", Category = "DX11.Texture", Version = "2d", Author = "velcrome")]
    public class PointerTextureNode : IPluginEvaluate, IDX11ResourceHost
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
            if (this.FPointer.IsChanged)
            {
                this.FInvalidate = true;
            }
            else return;

            SpreadMax = FPointer.SliceCount;

            var oldCount = FTextureOutput.SliceCount;
            if (FTextureOutput[0] == null) oldCount = 0;

            for (int i = SpreadMax; i < oldCount;i++ )
            {
                FTextureOutput[i].Dispose();
            }

            this.FValid.SliceCount = SpreadMax;
            this.FTextureOutput.SliceCount = SpreadMax;

            for (int i = oldCount; i < SpreadMax; i++)
            {
                FTextureOutput[i] = new DX11Resource<DX11Texture2D>();
            }


        }

        public void Update(DX11RenderContext context)
        {

            if (this.FInvalidate)
            {
                for (int i = 0; i < FTextureOutput.SliceCount; i++)
                {
                    if (this.FTextureOutput[i].Contains(context))
                    {
                        this.FTextureOutput[i].Dispose(context);
                    }

                    try
                    {
                        int p = unchecked((int) this.FPointer[i]);
                        IntPtr share = new IntPtr(p);
                        Texture2D tex = context.Device.OpenSharedResource<Texture2D>(share);
                        ShaderResourceView srv = new ShaderResourceView(context.Device, tex);

                        DX11Texture2D resource = DX11Texture2D.FromTextureAndSRV(context, tex, srv);

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
            for (int i = 0; i < FTextureOutput.SliceCount; i++)
                this.FTextureOutput[i].Dispose(context);
        }
    }
}
