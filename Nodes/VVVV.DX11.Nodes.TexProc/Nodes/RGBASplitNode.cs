using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Queries;
using SlimDX;
using FeralTic.DX11.Resources;
using System.Reflection;
using SlimDX.Direct3D11;
using FeralTic.DX11.Geometry;
using FeralTic.Core;
using VVVV.DX11.Nodes.TexProc;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "RGBASplit", Category = "DX11.Texture", Author = "vux")]
    public class RGBASplit : IPluginEvaluate, IDX11ResourceHost, IDX11Queryable, IDisposable
    {
        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> textureInput;

        [Input("Single Channel Output")]
        protected ISpread<bool> singleChannelOut;

        [Input("White If Invalid")]
        protected ISpread<bool> whiteIfInvalid;

        [Output("Texture Red")]
        protected ISpread<DX11Resource<DX11Texture2D>> textureRedOutput;

        [Output("Texture Green")]
        protected ISpread<DX11Resource<DX11Texture2D>> textureGreenOutput;

        [Output("Texture Blue")]
        protected ISpread<DX11Resource<DX11Texture2D>> textureBlueOutput;

        [Output("Texture Alpha")]
        protected ISpread<DX11Resource<DX11Texture2D>> textureAlphaOutput;

        [Output("Message", Order = 5)]
        protected ISpread<string> message;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> queryable;

        private List<DX11ResourcePoolEntry<DX11RenderTarget2D>> framePool = new List<DX11ResourcePoolEntry<DX11RenderTarget2D>>();

        private static DX11ShaderInstance shader = null;
        private static DX11ShaderInstance shaderExpand = null;

        public void Evaluate(int SpreadMax)
        {
            this.textureRedOutput.SliceCount = SpreadMax;
            this.textureGreenOutput.SliceCount = SpreadMax;
            this.textureBlueOutput.SliceCount = SpreadMax;
            this.textureAlphaOutput.SliceCount = SpreadMax;

            this.message.SliceCount = this.textureRedOutput.SliceCount;

            for (int i = 0; i < this.textureRedOutput.SliceCount; i++)
            {
                if (this.textureRedOutput[i] == null)
                {
                    this.textureRedOutput[i] = new DX11Resource<DX11Texture2D>();
                    this.textureGreenOutput[i] = new DX11Resource<DX11Texture2D>();
                    this.textureBlueOutput[i] = new DX11Resource<DX11Texture2D>();
                    this.textureAlphaOutput[i] = new DX11Resource<DX11Texture2D>();
                }
            }

            this.queryable[0] = this;
        } 

        #region IDX11ResourceProvider Members
        public void Update(DX11RenderContext context)
        {
            for (int i = 0; i < framePool.Count; i++)
            {
                framePool[i].UnLock();
            }
            framePool.Clear();

            if (this.textureRedOutput.SliceCount == 0)
            {
                return;
            }

            //

            //Compile shader if necessary
            if (shader == null)
            {
                using (DX11Effect effect = DX11Effect.FromResource(Assembly.GetExecutingAssembly(), Consts.EffectPath + ".RGBASplit.fx"))
                {
                    shader = new DX11ShaderInstance(context, effect);
                }
            }
            if (shaderExpand == null)
            {
                using (DX11Effect effect = DX11Effect.FromResource(Assembly.GetExecutingAssembly(), Consts.EffectPath + ".RGBASplitExpand.fx"))
                {
                    shaderExpand = new DX11ShaderInstance(context, effect);
                }
            }

            for (int i = 0; i < this.textureRedOutput.SliceCount; i++)
            {
                if (this.textureInput[i] == null)
                {
                    this.SetDefault(context, i);
                }
                else if (this.textureInput[i].Contains(context))
                {
                    var instance = this.singleChannelOut[i] ? shader : shaderExpand;

                    var input = this.textureInput[i][context];

                    string suffix = "Float";

                    var inputFormat = input.SRV.Description.Format;
                    inputFormat = inputFormat.DefaultOutputForCompressed();

                    if (inputFormat.IsSignedInt())
                    {
                        suffix = "UInt";
                    }
                    if (inputFormat.IsUnsignedInt())
                    {
                        suffix = "Int";
                    }

                    instance.SelectTechnique("Apply" + suffix);
                    instance.SetByName("InputTexture", input.SRV);

                    var outputFormat = inputFormat;
                    if (this.singleChannelOut[i])
                    {
                        var singleFormat = outputFormat.GetSingleChannelEquivalent();
                        if(singleFormat == SlimDX.DXGI.Format.Unknown)
                        {
                            this.message[i] = "Could not find a single channel format suitable for : " + outputFormat.ToString();
                            this.SetDefault(context, i);
                            continue;
                        }
                        else
                        {
                            outputFormat = singleFormat;
                        }
                    }


                    var outputRed = context.ResourcePool.LockRenderTarget(input.Width, input.Height, outputFormat, false, 1, false);
                    var outputGreen = context.ResourcePool.LockRenderTarget(input.Width, input.Height, outputFormat, false, 1, false);
                    var outputBlue = context.ResourcePool.LockRenderTarget(input.Width, input.Height, outputFormat, false, 1, false);
                    var outputAlpha = context.ResourcePool.LockRenderTarget(input.Width, input.Height, outputFormat, false, 1, false);



                    context.RenderTargetStack.Push(outputRed.Element, outputGreen.Element, outputBlue.Element, outputAlpha.Element);

                    context.Primitives.ApplyFullTriVSPosition();
                    instance.ApplyPass(0);
                    context.CurrentDeviceContext.Draw(3, 0);

                    context.RenderTargetStack.Pop();

                    this.framePool.Add(outputRed);
                    this.framePool.Add(outputGreen);
                    this.framePool.Add(outputBlue);
                    this.framePool.Add(outputAlpha);



                    this.textureRedOutput[i][context] = outputRed.Element;
                    this.textureGreenOutput[i][context] = outputGreen.Element;
                    this.textureBlueOutput[i][context] = outputBlue.Element;
                    this.textureAlphaOutput[i][context] = outputAlpha.Element;

                    this.message[i] = "ok";
                }
                else
                {
                    this.SetDefault(context, i);
                }
            }

            if (this.BeginQuery != null)
            {
                this.BeginQuery(context);
            }

            if (this.EndQuery != null)
            {
                this.EndQuery(context);
            }

        }

        private void SetDefault(DX11RenderContext context, int i)
        {
            var defaultTex = this.whiteIfInvalid[i] ? context.DefaultTextures.WhiteTexture : context.DefaultTextures.BlackTexture;
            this.textureRedOutput[i][context] = defaultTex;
            this.textureGreenOutput[i][context] = defaultTex;
            this.textureBlueOutput[i][context] = defaultTex;
            this.textureAlphaOutput[i][context] = defaultTex;
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
 
        }
        #endregion

        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        public void Dispose()
        {
            for (int i = 0; i < framePool.Count; i++)
            {
                framePool[i].UnLock();
            }
            framePool.Clear();
        }
    }
}
