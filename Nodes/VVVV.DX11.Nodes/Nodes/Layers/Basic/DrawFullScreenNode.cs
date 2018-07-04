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

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "DrawFullScreen", Category = "DX11.Layer", Author = "vux", Help ="Draws full screen element, ignores depth buffer")]
    public class DX11DrawFullScreenNode : IPluginEvaluate, IDX11LayerHost, IDX11Queryable
    {
#region Private data
        private class ShaderDeviceData : IDX11Resource
        {
            private DX11ShaderInstance shaderInstance;
            public EffectPass pass;

            public EffectResourceVariable inputTextureVariable;
            public EffectVectorVariable colorVariable;
            public EffectMatrixVariable texTransformVariable;
            public EffectSamplerVariable samplerVariable;


            public ShaderDeviceData(DX11RenderContext context)
            {
                string basepath = "VVVV.DX11.Nodes.effects.DrawFullScreen.fx";
                using (DX11Effect effect = DX11Effect.FromResource(Assembly.GetExecutingAssembly(), basepath))
                {
                    shaderInstance = new DX11ShaderInstance(context, effect);

                    inputTextureVariable = shaderInstance.Effect.GetVariableBySemantic("INPUTTEXTURE").AsResource();
                    colorVariable = shaderInstance.Effect.GetVariableBySemantic("COLOR").AsVector();
                    samplerVariable = shaderInstance.Effect.GetVariableBySemantic("TEXTURESAMPLER").AsSampler();
                    texTransformVariable = shaderInstance.Effect.GetVariableBySemantic("TEXTUREMATRIX").AsMatrix();
                    pass = shaderInstance.CurrentTechnique.GetPassByIndex(0);
                }
            }

            public void Dispose()
            {
                if (this.shaderInstance != null)
                {
                    this.shaderInstance.Dispose();
                    this.shaderInstance = null;
                }
            }
        }
#endregion

        [Input("Render State")]
        protected Pin<DX11RenderState> FInState;

        [Input("Blend State")]
        protected ISpread<BlendStatePreset> FInBlendState;

        [Input("Texture")]
        protected Pin<DX11Resource<DX11Texture2D>> FInTexture;

        [Input("Sampler State")]
        protected Pin<SamplerDescription> FInSamplerState;
        
        [Input("Texture Transform")]
        protected ISpread<Matrix> FInTexTransform;
        
        [Input("Color", DefaultColor= new double[] {1,1,1,1})]
        protected ISpread<Color4> FInColor;

        [Input("Enabled", DefaultValue = 1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        public event DX11QueryableDelegate BeginQuery;
        public event DX11QueryableDelegate EndQuery;

        private int spmax;
        private DX11Resource<ShaderDeviceData> shaderData = new DX11Resource<ShaderDeviceData>();

        private DX11RenderState defaultState = new DX11RenderState();

        [ImportingConstructor()]
        public DX11DrawFullScreenNode(IPluginHost host, IIOFactory iofactory)
        {
            this.defaultState.DepthStencil = DX11DepthStencilStates.GetState(DepthStencilStatePreset.NoDepth);
        }

        public void Evaluate(int SpreadMax)
        {
            this.spmax = SpreadMax;
            if (SpreadMax > 0)
            {
                if (this.FOutLayer.SliceCount == 0)
                {
                    this.FOutLayer.SliceCount = 1;
                }

                if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }
                if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }
            }
            else
            {
                this.FOutLayer.SliceCount = 0;
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (!shaderData.Contains(context))
            {
                shaderData[context] = new ShaderDeviceData(context);
            }

            if (this.spmax > 0)
            {
                if (!this.FOutLayer[0].Contains(context))
                {
                    this.FOutLayer[0][context] = new DX11Layer();
                    this.FOutLayer[0][context].Render = this.Render;
                }
            }
        }

        public void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.spmax == 0)
                return;

            if (this.BeginQuery != null)
            {
                this.BeginQuery(context);
            }

            context.CleanShaderStages();
            ShaderDeviceData deviceData = this.shaderData[context];

            context.Primitives.FullScreenTriangle.Bind(null);

            for (int i = 0; i < spmax; i++)
            {
                if (this.FEnabled[i])
                {
                    if (this.FInState.IsConnected)
                    {
                        context.RenderStateStack.Push(this.FInState[i]);
                    }
                    else
                    {
                        this.defaultState.Blend = DX11BlendStates.GetState(this.FInBlendState[i]);
                        context.RenderStateStack.Push(this.defaultState);
                    }

                    if (this.FInSamplerState.IsConnected)
                    {
                        SamplerState state = SamplerState.FromDescription(context.Device, this.FInSamplerState[i]);
                        deviceData.samplerVariable.SetSamplerState(0, state);
                    }
                    else
                    {
                        deviceData.samplerVariable.UndoSetSamplerState(0);
                    }

                    var color = this.FInColor[i];
                    color.Alpha *= settings.LayerOpacity;

                    deviceData.colorVariable.Set(color);
                    deviceData.texTransformVariable.SetMatrix(this.FInTexTransform[i]);
                    
                    if (this.FInTexture.IsConnected)
                    {
                        if (this.FInTexture[i].Contains(context) && this.FInTexture[i][context] != null)
                        {
                            deviceData.inputTextureVariable.SetResource(this.FInTexture[i][context].SRV);
                        }
                        else
                        {
                            deviceData.inputTextureVariable.SetResource(null);
                        }
                    }
                    else
                    {
                        deviceData.inputTextureVariable.SetResource(context.DefaultTextures.WhiteTexture.SRV);
                    }


                    deviceData.pass.Apply(context.CurrentDeviceContext);
                    context.CurrentDeviceContext.Draw(3, 0);

                    context.RenderStateStack.Pop();
                }
            }

            if (this.EndQuery != null)
            {
                this.EndQuery(context);
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }

        public void Dispose()
        {
            this.FOutLayer.SafeDisposeAll();
            this.shaderData.Dispose();
        }
    }
}
