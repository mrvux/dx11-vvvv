using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using SlimDX.DXGI;
using VVVV.DX11.Lib.Rendering;

using FeralTic.DX11;
using FeralTic.DX11.Resources;


namespace VVVV.DX11
{
    


    [PluginInfo(Name = "Renderer", Category = "DX11", Version = "TempTarget", Author = "vux,tonfilm", AutoEvaluate = false)]
    public class DX11TempRTRendererNode : AbstractDX11Renderer2DNode
    {
        #region Inputs
        [Input("Generate Mip Maps", Order = 4)]
        protected IDiffSpread<bool> FInDoMipMaps;

        [Input("Mip Map Levels", Order = 5)]
        protected IDiffSpread<int> FInMipLevel;

        [Input("Shared Texture", Order = 6)]
        protected IDiffSpread<bool> FInSharedTex;
        #endregion

        #region Output Pins
        [Output("Buffer Size")]
        protected ISpread<Vector2D> FOutBufferSize;

        [Output("Buffers", IsSingle = true)]
        protected ISpread<DX11Resource<DX11RenderTarget2D>> FOutBuffers;

        [Output("AA Texture Out", IsSingle = true, Visibility=PinVisibility.OnlyInspector)]
        protected ISpread<DX11Resource<DX11RenderTarget2D>> FOutAABuffers;

        #endregion

        private bool genmipmap;
        private int mipmaplevel;

        private bool invalidate = true;

        private Dictionary<DX11RenderContext, DX11RenderTarget2D> targets = new Dictionary<DX11RenderContext, DX11RenderTarget2D>();
        private Dictionary<DX11RenderContext, DX11RenderTarget2D> targetresolve = new Dictionary<DX11RenderContext, DX11RenderTarget2D>();
        private RenderTargetManager rtm;
        private SlimDX.DXGI.Format format;

        #region Constructor
        [ImportingConstructor()]
        public DX11TempRTRendererNode(IPluginHost FHost, IIOFactory iofactory)
        {
            this.depthmanager = new DepthBufferManager(FHost,iofactory);
            this.rtm = new RenderTargetManager(FHost,iofactory);
        }
        #endregion

        #region On Evaluate
        protected override void OnEvaluate(int SpreadMax)
        {
            if (this.FOutBuffers[0] == null) { this.FOutBuffers[0] = new DX11Resource<DX11RenderTarget2D>(); }
            if (this.FOutAABuffers[0] == null) { this.FOutAABuffers[0] = new DX11Resource<DX11RenderTarget2D>(); }

            if (this.FInAASamplesPerPixel.IsChanged
              || this.FInDoMipMaps.IsChanged
              || this.FInMipLevel.IsChanged
              || this.FInSharedTex.IsChanged
              || this.rtm.FInTextureSize.IOObject.IsChanged
              || this.rtm.FInTextureScale.IOObject.IsChanged)

            {
                this.sd.Count = Convert.ToInt32(this.FInAASamplesPerPixel[0].Name);
                this.sd.Quality = 0;
                this.genmipmap = this.FInDoMipMaps[0];
                this.mipmaplevel = Math.Max(FInMipLevel[0], 0);
                this.depthmanager.NeedReset = true;
                this.invalidate = true;

                this.FOutBuffers.SafeUnlock();
                this.FOutAABuffers.SafeUnlock();
                this.targets.Clear();
                this.targetresolve.Clear();
            }

            this.FOutBufferSize[0] = new Vector2D(this.width, this.height);          
        }
        #endregion

        #region On Update
        protected override void OnUpdate(DX11RenderContext context)
        {
            //Grab a temp target if enabled

            TexInfo ti = this.rtm.GetRenderTarget(context);

            if (ti.w != this.width || ti.h != this.height || !this.targets.ContainsKey(context) || this.invalidate
                || ti.format !=this.format)
            {
                this.invalidate = false;
                this.width = ti.w;
                this.height = ti.h;
                this.format = ti.format;

                this.depthmanager.NeedReset = true;

                if (targets.ContainsKey(context))
                {
                    context.ResourcePool.Unlock(targets[context]);
                }

                if (targetresolve.ContainsKey(context))
                {
                    context.ResourcePool.Unlock(targetresolve[context]);
                }

                int aacount = Convert.ToInt32(this.FInAASamplesPerPixel[0].Name);
                int aaquality = 0;

                if (aacount > 1)
                {
                    List<SampleDescription> sds = context.GetMultisampleFormatInfo(ti.format);
                    int maxlevels = sds[sds.Count - 1].Count;

                    if (aacount > maxlevels)
                    {
                        FHost.Log(TLogType.Warning, "Multisample count too high for this format, reverted to: " + maxlevels);
                        aacount = maxlevels;
                    }

                    DX11RenderTarget2D temptarget = context.ResourcePool.LockRenderTarget(this.width, this.height, ti.format, new SampleDescription(aacount, aaquality), this.FInDoMipMaps[0], this.FInMipLevel[0], this.FInSharedTex[0]).Element;
                    DX11RenderTarget2D temptargetresolve = context.ResourcePool.LockRenderTarget(this.width, this.height, ti.format, new SampleDescription(1, 0), this.FInDoMipMaps[0], this.FInMipLevel[0], this.FInSharedTex[0]).Element;

                    targets[context] = temptarget;
                    targetresolve[context] = temptargetresolve;

                    this.FOutBuffers[0][context] = temptargetresolve;
                    this.FOutAABuffers[0][context] = temptarget;
                }
                else
                {
                    //Bind both texture as same output
                    DX11RenderTarget2D temptarget = context.ResourcePool.LockRenderTarget(this.width, this.height, ti.format, new SampleDescription(aacount, aaquality), this.FInDoMipMaps[0], this.FInMipLevel[0], this.FInSharedTex[0]).Element;
                    targets[context] = temptarget;
  
                    this.FOutBuffers[0][context] = temptarget;
                    this.FOutAABuffers[0][context] = temptarget;

                }



            }



        }
        #endregion

        protected override IDX11RWResource GetMainTarget(DX11RenderContext context)
        {
            return this.FOutBuffers[0][context] as IDX11RWResource;
        }

        #region On Destroy
        protected override void OnDestroy(DX11RenderContext context, bool force)
        {
            //Release lock on target
            if (targets.ContainsKey(context))
            {
                context.ResourcePool.Unlock(targets[context]);
                targets.Remove(context);
            }

            if (targetresolve.ContainsKey(context))
            {
                context.ResourcePool.Unlock(targetresolve[context]);
                targetresolve.Remove(context);
            }

            

        }
        #endregion

        #region Before Render
        protected override void BeforeRender(DX11GraphicsRenderer renderer, DX11RenderContext context)
        {
            renderer.EnableDepth = this.FInDepthBuffer[0];
            renderer.DepthStencil = this.depthmanager.GetDepthStencil(context);
            renderer.DepthMode = this.depthmanager.Mode;
            renderer.SetRenderTargets(targets[context]);
        }
        #endregion

        #region After Render
        protected override void AfterRender(DX11GraphicsRenderer renderer, DX11RenderContext context)
        {
            if (this.sd.Count > 1)
            {
                context.CurrentDeviceContext.ResolveSubresource(targets[context].Resource, 0, targetresolve[context].Resource,
                    0, targets[context].Format);
            }

            if (this.genmipmap && this.sd.Count == 1)
            {
                for (int i = 0; i < this.FOutBuffers.SliceCount; i++)
                {
                    context.CurrentDeviceContext.GenerateMips(targets[context].SRV);
                }
            }
        }
        #endregion

        #region Dispose
        protected override void OnDispose()
        {
            foreach (var tg in targets)
            {
                tg.Key.ResourcePool.Unlock(tg.Value);
            }
            foreach (var tg in targetresolve)
            {
                tg.Key.ResourcePool.Unlock(tg.Value);
            }
            targets.Clear();
            targetresolve.Clear();
        }
        #endregion
    }
}
