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
    public class PointerTextureNode : IPluginEvaluate, IDX11ResourceProvider
    {
        [Input("Pointer", IsSingle=true)]
        protected IDiffSpread<uint> FPointer;

        [Output("Texture")]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureOutput;

        [Output("Is Valid", IsSingle=true)]
        protected ISpread<bool> FValid;

        private bool FInvalidate;

        public void Evaluate(int SpreadMax)
        {
            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11Texture2D>();
            }

            this.FValid.SliceCount = 1;


            if (this.FPointer.IsChanged)
            {
                this.FInvalidate = true;
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {

            if (this.FInvalidate)
            {
                if (this.FTextureOutput[0].Contains(context))
                {
                    this.FTextureOutput[0].Dispose(context);
                }

                try
                {
                	int p = unchecked((int) this.FPointer[0]);
                    Texture2D tex = context.Device.OpenSharedResource<Texture2D>(new IntPtr(p));
                    ShaderResourceView srv = new ShaderResourceView(context.Device, tex);

                    DX11Texture2D resource = DX11Texture2D.FromTextureAndSRV(context, tex, srv);

                    this.FTextureOutput[0][context] = resource;

                    this.FValid[0] = true;
                }
                catch (Exception ex)
                {
                    this.FValid[0] = false;
                }

                this.FInvalidate = false;
            }           
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            this.FTextureOutput[0].Dispose(context);
        }
    }
}
