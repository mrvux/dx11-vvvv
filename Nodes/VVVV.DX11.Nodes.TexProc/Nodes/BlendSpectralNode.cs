using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeralTic.DX11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using FeralTic.DX11.Resources;
using SlimDX;
using VVVV.DX11.Nodes.TexProc;
using FeralTic;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Composite", Category = "DX11.Texture", Version = "Spectral")]
    public class BlendSpectralNode : IPluginEvaluate, IDX11ResourceHost, System.ComponentModel.Composition.IPartImportsSatisfiedNotification, IDisposable
    {
        protected enum modeEnum 
        {
            Normal,
            Screen,
            Multiply,
            Add,
            Subtract,
            Darken,
            Lighten,
            Difference,
            Exclusion,
            Overlay
        }

        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> texture1;

        [Input("Texture Transform")]
        protected ISpread<Matrix> textureTransform;

        [Input("Alpha", MinValue =0.0f, MaxValue =1.0f)]
        protected ISpread<float> alpha;

        [Input("Mode")]
        protected ISpread<modeEnum> mode;

        [Input("Size", DefaultValues = new double[] { 640, 480 }, AsInt = true)]
        protected ISpread<Vector2> size;

        [Input("Auto Texture Size", DefaultValue = 1)]
        protected ISpread<bool> firstTextureAsSize;

        [Output("Texture Output")]
        protected ISpread<DX11Resource<DX11Texture2D>> textureOutput;

        private int spreadMax;
        private DX11Effect effect;
        private DX11ShaderInstance instance;

        private DX11Effect vertexShadersEffect;
        private DX11ShaderInstance vertexShadersInstance;

        private DX11ResourcePoolEntry<DX11RenderTarget2D> resourceEntry;

        public void OnImportsSatisfied()
        {
            effect = DX11Effect.FromResource(System.Reflection.Assembly.GetExecutingAssembly(), Consts.EffectPath + ".Composite.fx");
            vertexShadersEffect = DX11Effect.FromResource(System.Reflection.Assembly.GetExecutingAssembly(), Consts.EffectPath + ".VSFullTriDualTexTransform.fx");
        }

        public void Evaluate(int SpreadMax)
        {
            this.spreadMax = SpreadMax;
            if (this.textureOutput[0] == null)
            {
                this.textureOutput[0] = new DX11Resource<DX11Texture2D>();
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (resourceEntry != null)
            {
                resourceEntry.UnLock();
                resourceEntry = null;
            }

            if (!texture1.IsConnected)
            {
                this.textureOutput[0][context] = context.DefaultTextures.WhiteTexture;
                return;
            }

            if (this.texture1.SliceCount == 1)
            {
                this.textureOutput[0][context] = this.texture1[0][context];
                return;
            }

            if (instance == null)
            {
                instance = new DX11ShaderInstance(context, effect);
                vertexShadersInstance = new DX11ShaderInstance(context, vertexShadersEffect);
            }

            var width = this.firstTextureAsSize[0] ? texture1[0][context].Width : (int)size[0].X;
            var height = this.firstTextureAsSize[0] ? texture1[0][context].Height : (int)size[0].Y;


            DX11ResourcePoolEntry <DX11RenderTarget2D> resourceRead = context.ResourcePool.LockRenderTarget(width, height, SlimDX.DXGI.Format.R8G8B8A8_UNorm);
            DX11ResourcePoolEntry<DX11RenderTarget2D> resourceWrite = context.ResourcePool.LockRenderTarget(width, height, SlimDX.DXGI.Format.R8G8B8A8_UNorm);
            bool first = true;

            context.Primitives.FullScreenTriangle.Bind(null);

            for (int i = 0; i < this.texture1.SliceCount - 1; i++)
            {
                context.RenderTargetStack.Push(resourceWrite.Element);

                //First pass we need to apply both texture transform, next only second
                if (!first)
                {
                    instance.SetBySemantic("INPUTTEXTURE", resourceRead.Element.SRV);
                    vertexShadersInstance.SelectTechnique("ApplySecondOnly");
                }
                else
                {
                    instance.SetBySemantic("INPUTTEXTURE", texture1[0][context].SRV);
                    vertexShadersInstance.SelectTechnique("ApplyBoth");
                    vertexShadersInstance.SetByName("tTex1", this.textureTransform[0].AsTextureTransform());
                }

                instance.SetBySemantic("SECONDTEXTURE", texture1[i+1][context].SRV);
                vertexShadersInstance.SetByName("tTex2", this.textureTransform[i+1].AsTextureTransform());


                instance.SelectTechnique(mode[i].ToString());
                instance.SetByName("Opacity", alpha[i]);

                vertexShadersInstance.ApplyPass(0);
                instance.ApplyPass(0);


                context.Primitives.FullScreenTriangle.Draw();

                context.RenderTargetStack.Pop();
                first = false;

                var tmp = resourceWrite;
                resourceWrite = resourceRead;
                resourceRead = tmp;
            }

            resourceWrite.UnLock();

            resourceEntry = resourceRead;
            this.textureOutput[0][context] = resourceRead.Element;
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }

        public void Dispose()
        {
            if (resourceEntry != null)
            {
                resourceEntry.UnLock();
                resourceEntry = null;
            }
            this.effect.Dispose();
            this.vertexShadersEffect.Dispose();
            if (this.vertexShadersInstance != null)
            {
                this.instance.Dispose();
                this.vertexShadersInstance.Dispose();
            }
        }
    }
}
