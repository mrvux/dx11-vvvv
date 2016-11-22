using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using VVVV.DX11.Lib;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "FileTexture", Category = "DX11", Version = "2d.Pooled", Author = "vux")]
    public class FileTexturePoolNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Filename", StringType = StringType.Filename)]
        protected IDiffSpread<string> FInPath;

        [Input("Load In Background", IsSingle = true)]
        protected ISpread<bool> FInBGLoad;

        [Input("Reload", IsBang = true)]
        protected ISpread<bool> FInReload;

        [Input("Keep In Memory", Visibility = PinVisibility.Hidden)]
        protected ISpread<bool> FInKeep;

        [Input("No Mips", Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<bool> FInNoMips;

        [Output("Texture Out")]
        protected ISpread<DX11Resource<DX11Texture2D>> FTextureOutput;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private Dictionary<DX11RenderContext, DX11FileTexturePool> pools = new Dictionary<DX11RenderContext, DX11FileTexturePool>();

        private bool FInvalidate = true;
        private int spmax;

        public void Evaluate(int SpreadMax)
        {
            this.spmax = SpreadMax;
            if (this.FInPath.IsChanged || this.FInReload[0])
            {
                this.FInvalidate = true;
            }

            this.FTextureOutput.SliceCount = SpreadMax;
            this.FValid.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FTextureOutput[i] == null)
                {
                    this.FTextureOutput[i] = new DX11Resource<DX11Texture2D>();
                }             
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInvalidate || !this.pools.ContainsKey(context))
            {
                if (!this.pools.ContainsKey(context))
                {
                    DX11FileTexturePool fp = new DX11FileTexturePool();
                    fp.OnElementLoaded += fp_OnElementLoaded;

                    this.pools.Add(context, fp);
                }

                DX11FileTexturePool pool = this.pools[context];

                pool.DecrementAll();
                //Update pins
                for (int i = 0; i < this.spmax;i++)
                {
                    DX11Texture2D result;
                    bool valid = pool.TryGetFile(context, this.FInPath[i], !FInNoMips[i],FInBGLoad[i], out result);

                    this.FValid[i] = valid;

                    if (valid)
                    {
                        this.FTextureOutput[i][context] = result;
                    }
                    else
                    {
                        this.FTextureOutput[i].Remove(context);
                    }
                    

                }

                pool.Flush();
                this.FInvalidate = false;
            }
        }

        void fp_OnElementLoaded(object sender, EventArgs e)
        {
            this.FInvalidate = true;
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (force || !this.FInKeep[0])
            {
                if (this.pools.ContainsKey(context))
                {
                    this.pools[context].Dispose();
                    this.pools.Remove(context);
                }
            }
        }

        public void Dispose()
        {
            foreach (DX11FileTexturePool pool in this.pools.Values)
            {
                pool.Dispose();
            }
        }
    }
}
