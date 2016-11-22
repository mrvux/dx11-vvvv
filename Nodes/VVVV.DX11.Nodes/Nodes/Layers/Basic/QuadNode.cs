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
    [PluginInfo(Name = "Quad", Category = "DX11.Layer", Author = "vux")]
    public class DX11QuadLayerNode : IPluginEvaluate, IDX11LayerHost, IDX11Queryable
    {
        [Input("Render State")]
        protected Pin<DX11RenderState> FInState;

        [Input("Transform")]
        protected ISpread<Matrix> FInWorld;

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

        private int spmax;

        /*private static DX11ShaderInstance quadshader;

        private static List<InputLayout> quadlayouts;

        private static DX11IndexedGeometry quadgeometry;*/

        /*private DX11DynamicStructuredBuffer<Matrix> worldbuffer;
        private DX11DynamicStructuredBuffer<Color4> colorbuffer;
        private DX11DynamicStructuredBuffer<Matrix> uvbuffer;*/

        /*private static EffectResourceVariable texturevariable;
        private static EffectSamplerVariable samplervariable;*/

        private class QuadBuffers
        {
            public DX11DynamicStructuredBuffer<Matrix> worldbuffer;
            public DX11DynamicStructuredBuffer<Color4> colorbuffer;
            public DX11DynamicStructuredBuffer<Matrix> uvbuffer;
        }

        private class QuadShaderDeviceData
        {
            public DX11ShaderInstance quadshader;
            public List<InputLayout> quadlayouts;
            public DX11IndexedGeometry quadgeometry;
            public EffectResourceVariable texturevariable;
            public EffectSamplerVariable samplervariable;

            public QuadShaderDeviceData(DX11RenderContext context)
            {
                string basepath = "VVVV.DX11.Nodes.effects.quad.fx";
                DX11Effect effect = DX11Effect.FromResource(Assembly.GetExecutingAssembly(), basepath);

                quadshader = new DX11ShaderInstance(context, effect);
                texturevariable = quadshader.Effect.GetVariableBySemantic("INPUTTEXTURE").AsResource();
                samplervariable = quadshader.Effect.GetVariableBySemantic("SAMPLERSTATE").AsSampler();

                Quad quad = new Quad();
                quad.Size = new Vector2(1.0f);

                quadgeometry = context.Primitives.QuadNormals(quad);

                quadlayouts = new List<InputLayout>();
                for (int i = 0; i < 4; i++)
                {
                    InputLayout layout;
                    quadshader.SelectTechnique(i);

                    bool res = quadgeometry.ValidateLayout(quadshader.GetPass(0), out layout);
                    quadlayouts.Add(layout);
                }
            }
        }

        private static Dictionary<DX11RenderContext, QuadShaderDeviceData> quaddata = new Dictionary<DX11RenderContext, QuadShaderDeviceData>();
        private Dictionary<DX11RenderContext, QuadBuffers> quadBuffers = new Dictionary<DX11RenderContext, QuadBuffers>();

        private object syncRoot = new object();

        [ImportingConstructor()]
        public DX11QuadLayerNode(IPluginHost host, IIOFactory iofactory)
        {

        }

        public void Evaluate(int SpreadMax)
        {
            this.spmax = SpreadMax;
            if (SpreadMax > 0)
            {
                if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }
                if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }
            }
        }

        #region IDX11ResourceProvider Members
        public void Update(DX11RenderContext context)
        {
            lock( syncRoot)
            {
                if (!quaddata.ContainsKey(context))
                {
                    quaddata[context] = new QuadShaderDeviceData(context);
                }

                if (!quadBuffers.ContainsKey(context))
                {
                    quadBuffers[context] = new QuadBuffers();
                }
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

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutLayer[0].Dispose(context);
        }

        private void RenderBasic(DX11RenderContext context, DX11RenderSettings settings)
        {
            QuadShaderDeviceData qd = quaddata[context];

            qd.quadshader.SelectTechnique("Render");
            qd.quadgeometry.Bind(qd.quadlayouts[0]);

            for (int i = 0; i < this.spmax; i++)
            {
                bool popstate = false;
                if (this.FInState.IsConnected)
                {
                    context.RenderStateStack.Push(this.FInState[i]);
                    popstate = true;
                }

                qd.quadshader.SetBySemantic("COLOR", this.FInColor[i]);
                qd.quadshader.SetBySemantic("WORLD", this.FInWorld[i]);
                qd.quadshader.SetBySemantic("TEXTUREMATRIX", this.FInTexTransform[i]);

                qd.quadshader.ApplyPass(0);
                if (settings.DepthOnly)
                {
                    context.CurrentDeviceContext.PixelShader.Set(null);
                }

                    qd.quadgeometry.Draw();

                if (popstate) { context.RenderStateStack.Pop(); }
            }
        }


        private void RenderTextured(DX11RenderContext context, DX11RenderSettings settings)
        {
            QuadShaderDeviceData qd = quaddata[context];
            qd.quadshader.SelectTechnique("RenderTextured");
            qd.quadgeometry.Bind(qd.quadlayouts[1]);

            for (int i = 0; i < this.spmax; i++)
            {
                bool popstate = false;
                if (this.FInState.IsConnected)
                {
                    context.RenderStateStack.Push(this.FInState[i]);
                    popstate = true;
                }

                if (this.FInSamplerState.IsConnected)
                {
                    SamplerState state = SamplerState.FromDescription(context.Device, this.FInSamplerState[i]);
                    qd.samplervariable.SetSamplerState(0, state);
                }
                else
                {
                    qd.samplervariable.UndoSetSamplerState(0);
                }

                qd.quadshader.SetBySemantic("COLOR", this.FInColor[i]);
                qd.quadshader.SetBySemantic("WORLD", this.FInWorld[i]);
                qd.quadshader.SetBySemantic("TEXTUREMATRIX", this.FInTexTransform[i]);

                if (this.FInTexture[i].Contains(context) && this.FInTexture[i][context] != null)
                {
                    qd.texturevariable.SetResource(this.FInTexture[i][context].SRV);
                }
                else
                {
                    qd.texturevariable.SetResource(null);
                }

                qd.quadshader.ApplyPass(0);
                if (settings.DepthOnly)
                {
                    context.CurrentDeviceContext.PixelShader.Set(null);
                }
                qd.quadgeometry.Draw();

                if (popstate) { context.RenderStateStack.Pop(); }
            }
        }


        private void RenderInstanced(DX11RenderContext context, DX11RenderSettings settings)
        {
            QuadShaderDeviceData qd = quaddata[context];
            bool popstate = false;
            if (this.FInState.IsConnected)
            {
                context.RenderStateStack.Push(this.FInState[0]);
                popstate = true;
            }

            qd.quadshader.SelectTechnique("RenderInstanced");
            qd.quadgeometry.Bind(qd.quadlayouts[2]);

            this.BindBuffers(context);

            qd.quadshader.ApplyPass(0);
            if (settings.DepthOnly)
            {
                context.CurrentDeviceContext.PixelShader.Set(null);
            }

            context.CurrentDeviceContext.DrawIndexedInstanced(qd.quadgeometry.IndexBuffer.IndicesCount, this.spmax, 0, 0, 0);

            if (popstate) { context.RenderStateStack.Pop(); }
        }


        private void RenderInstancedTextured(DX11RenderContext context, DX11RenderSettings settings)
        {
            QuadShaderDeviceData qd = quaddata[context];
            bool popstate = false;
            if (this.FInState.IsConnected)
            {
                context.RenderStateStack.Push(this.FInState[0]);
                popstate = true;
            }

            if (this.FInSamplerState.IsConnected)
            {
                SamplerState state = SamplerState.FromDescription(context.Device,this.FInSamplerState[0]);
                qd.samplervariable.SetSamplerState(0, state);
            }
            else
            {
                qd.samplervariable.UndoSetSamplerState(0);
            }

            qd.quadshader.SelectTechnique("RenderInstancedTextured");
            qd.quadgeometry.Bind(qd.quadlayouts[3]);

            this.BindBuffers(context);

            if (this.FInTexture[0].Contains(context) && this.FInTexture[0][context] != null)
            {
                qd.texturevariable.SetResource(this.FInTexture[0][context].SRV);
            }
            else
            {
                qd.texturevariable.SetResource(null);
            }

            qd.quadshader.ApplyPass(0);
            if (settings.DepthOnly)
            {
                context.CurrentDeviceContext.PixelShader.Set(null);
            }

            context.CurrentDeviceContext.DrawIndexedInstanced(qd.quadgeometry.IndexBuffer.IndicesCount, this.spmax, 0, 0, 0);

            if (popstate) { context.RenderStateStack.Pop(); }
        }

        private void BindBuffers(DX11RenderContext context)
        {
            QuadShaderDeviceData qd = quaddata[context];
            QuadBuffers qb = quadBuffers[context];
            if (qb.worldbuffer != null)
            {
                if (qb.worldbuffer.ElementCount != this.FInWorld.SliceCount)
                {
                    qb.worldbuffer.Dispose(); qb.worldbuffer = null;
                }
            }

            if (qb.colorbuffer != null)
            {
                if (qb.colorbuffer.ElementCount != this.FInColor.SliceCount)
                {
                    qb.colorbuffer.Dispose(); qb.colorbuffer = null;
                }
            }

            if (this.FInTexture.IsConnected)
            {
                if (qb.uvbuffer != null)
                {
                    if (qb.uvbuffer.ElementCount != this.FInTexTransform.SliceCount)
                    {
                        qb.uvbuffer.Dispose(); qb.uvbuffer = null;
                    }
                }
                if (qb.uvbuffer == null) { qb.uvbuffer = new DX11DynamicStructuredBuffer<Matrix>(context, this.FInTexTransform.SliceCount); }

                qd.quadshader.SetBySemantic("TEXTUREMATRIXCOUNT", qb.uvbuffer.ElementCount);
                qd.quadshader.SetBySemantic("TEXTUREMATRIXBUFFER", qb.uvbuffer.SRV);

                qb.uvbuffer.WriteData(this.FInTexTransform.Stream.Buffer, 0, this.FInTexTransform.SliceCount);
            }

            if (qb.worldbuffer == null) { qb.worldbuffer = new DX11DynamicStructuredBuffer<Matrix>(context, this.FInWorld.SliceCount); }
            if (qb.colorbuffer == null) { qb.colorbuffer = new DX11DynamicStructuredBuffer<Color4>(context, this.FInColor.SliceCount); }


            qb.worldbuffer.WriteData(this.FInWorld.Stream.Buffer, 0, this.FInWorld.SliceCount);
            qb.colorbuffer.WriteData(this.FInColor.Stream.Buffer, 0, this.FInColor.SliceCount);

            qd.quadshader.SetBySemantic("WORLDBUFFER", qb.worldbuffer.SRV);
            qd.quadshader.SetBySemantic("COLORBUFFER", qb.colorbuffer.SRV);

            qd.quadshader.SetBySemantic("WORLDCOUNT", qb.worldbuffer.ElementCount);
            qd.quadshader.SetBySemantic("COLORCOUNT", qb.colorbuffer.ElementCount);
            
        }


        public void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.spmax > 0)
            {
                if (this.FEnabled[0])
                {
                    context.CleanShaderStages();
                    QuadShaderDeviceData qd = quaddata[context];
                    qd.quadshader.SetBySemantic("VIEWPROJECTION", settings.ViewProjection);

                    if (this.BeginQuery != null)
                    {
                        this.BeginQuery(context);
                    }

                    bool multisampler = this.FInSamplerState.SliceCount > 1 && this.FInTexture.IsConnected;

                    if (this.FInState.SliceCount > 1 || this.FInTexture.SliceCount > 1 || multisampler)
                    {
                        if (this.FInTexture.IsConnected)
                        {
                            this.RenderTextured(context, settings);
                        }
                        else
                        {
                            this.RenderBasic(context, settings);
                        }
                    }
                    else
                    {
                        if (this.FInTexture.IsConnected)
                        {
                            this.RenderInstancedTextured(context, settings);
                        }
                        else
                        {
                            this.RenderInstanced(context, settings);
                        }
                    }


                    if (this.EndQuery != null)
                    {
                        this.EndQuery(context);
                    }
                }
            }
        }

        #endregion

        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;
    }
}
