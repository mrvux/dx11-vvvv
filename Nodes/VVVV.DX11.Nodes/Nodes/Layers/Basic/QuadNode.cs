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
    public class DX11QuadLayerNode : IPluginEvaluate, IDX11LayerProvider, IDX11Queryable
    {
        [Input("Render State")]
        protected Pin<DX11RenderState> FInState;

        [Input("Transform")]
        protected ISpread<Matrix> FInWorld;

        [Input("Texture")]
        protected Pin<DX11Resource<DX11Texture2D>> FInTexture;

        [Input("Samper State")]
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

        private static DX11ShaderInstance quadshader;

        private static List<InputLayout> quadlayouts;

        private static DX11IndexedGeometry quadgeometry;

        private DX11DynamicStructuredBuffer<Matrix> worldbuffer;
        private DX11DynamicStructuredBuffer<Color4> colorbuffer;
        private DX11DynamicStructuredBuffer<Matrix> uvbuffer;

        private static EffectResourceVariable texturevariable;
        private static EffectSamplerVariable samplervariable;


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
        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (quadshader == null)
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

            if (this.spmax > 0)
            {
                if (!this.FOutLayer[0].Contains(context))
                {
                    this.FOutLayer[0][context] = new DX11Layer();
                    this.FOutLayer[0][context].Render = this.Render;
                }
            }
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            this.FOutLayer[0].Dispose(context);
        }

        private void RenderBasic(DX11RenderContext context)
        {
            quadshader.SelectTechnique("Render");
            quadgeometry.Bind(quadlayouts[0]);

            for (int i = 0; i < this.spmax; i++)
            {
                bool popstate = false;
                if (this.FInState.PluginIO.IsConnected)
                {
                    context.RenderStateStack.Push(this.FInState[i]);
                    popstate = true;
                }

                quadshader.SetBySemantic("COLOR", this.FInColor[i]);
                quadshader.SetBySemantic("WORLD", this.FInWorld[i]);
                quadshader.SetBySemantic("TEXTUREMATRIX", this.FInTexTransform[i]);

                quadshader.ApplyPass(0);

                quadgeometry.Draw();

                if (popstate) { context.RenderStateStack.Pop(); }
            }
        }

        private void RenderTextured(DX11RenderContext context)
        {
            quadshader.SelectTechnique("RenderTextured");
            quadgeometry.Bind(quadlayouts[1]);

            for (int i = 0; i < this.spmax; i++)
            {
                bool popstate = false;
                if (this.FInState.PluginIO.IsConnected)
                {
                    context.RenderStateStack.Push(this.FInState[i]);
                    popstate = true;
                }

                if (this.FInSamplerState.PluginIO.IsConnected)
                {
                    SamplerState state = SamplerState.FromDescription(context.Device, this.FInSamplerState[i]);
                    samplervariable.SetSamplerState(0, state);
                }
                else
                {
                    samplervariable.UndoSetSamplerState(0);
                }

                quadshader.SetBySemantic("COLOR", this.FInColor[i]);
                quadshader.SetBySemantic("WORLD", this.FInWorld[i]);
                quadshader.SetBySemantic("TEXTUREMATRIX", this.FInTexTransform[i]);

                if (this.FInTexture[i].Contains(context))
                {
                    texturevariable.SetResource(this.FInTexture[i][context].SRV);
                }
                else
                {
                    texturevariable.SetResource(null);
                }

                quadshader.ApplyPass(0);

                quadgeometry.Draw();

                if (popstate) { context.RenderStateStack.Pop(); }
            }
        }


        private void RenderInstanced(DX11RenderContext context)
        {
            bool popstate = false;
            if (this.FInState.PluginIO.IsConnected)
            {
                context.RenderStateStack.Push(this.FInState[0]);
                popstate = true;
            }

            quadshader.SelectTechnique("RenderInstanced");
            quadgeometry.Bind(quadlayouts[2]);

            this.BindBuffers(context);

            quadshader.ApplyPass(0);

            context.CurrentDeviceContext.DrawIndexedInstanced(quadgeometry.IndexBuffer.IndicesCount, this.spmax, 0, 0, 0);

            if (popstate) { context.RenderStateStack.Pop(); }
        }


        private void RenderInstancedTextured(DX11RenderContext context)
        {
            bool popstate = false;
            if (this.FInState.PluginIO.IsConnected)
            {
                context.RenderStateStack.Push(this.FInState[0]);
                popstate = true;
            }

            if (this.FInSamplerState.PluginIO.IsConnected)
            {
                SamplerState state = SamplerState.FromDescription(context.Device,this.FInSamplerState[0]);
                samplervariable.SetSamplerState(0, state);
            }
            else
            {
                samplervariable.UndoSetSamplerState(0);
            }

            quadshader.SelectTechnique("RenderInstancedTextured");
            quadgeometry.Bind(quadlayouts[3]);

            this.BindBuffers(context);

            if (this.FInTexture[0].Contains(context))
            {
                texturevariable.SetResource(this.FInTexture[0][context].SRV);
            }
            else
            {
                texturevariable.SetResource(null);
            }

            quadshader.ApplyPass(0);

            context.CurrentDeviceContext.DrawIndexedInstanced(quadgeometry.IndexBuffer.IndicesCount, this.spmax, 0, 0, 0);

            if (popstate) { context.RenderStateStack.Pop(); }
        }

        private void BindBuffers(DX11RenderContext context)
        {
            if (this.worldbuffer != null)
            {
                if (this.worldbuffer.ElementCount != this.FInWorld.SliceCount)
                {
                    this.worldbuffer.Dispose(); this.worldbuffer = null;
                }
            }

            if (this.colorbuffer != null)
            {
                if (this.colorbuffer.ElementCount != this.FInColor.SliceCount)
                {
                    this.colorbuffer.Dispose(); this.colorbuffer = null;
                }
            }

            if (this.FInTexture.PluginIO.IsConnected)
            {
                if (this.uvbuffer != null)
                {
                    if (this.uvbuffer.ElementCount != this.FInTexTransform.SliceCount)
                    {
                        this.uvbuffer.Dispose(); this.uvbuffer = null;
                    }
                }
                if (this.uvbuffer == null) { this.uvbuffer = new DX11DynamicStructuredBuffer<Matrix>(context, this.FInTexTransform.SliceCount); }

                quadshader.SetBySemantic("TEXTUREMATRIXCOUNT", this.uvbuffer.ElementCount);
                quadshader.SetBySemantic("TEXTUREMATRIXBUFFER", this.uvbuffer.SRV);

                this.uvbuffer.WriteData(this.FInTexTransform.ToArray());
            }

            if (this.worldbuffer == null) { this.worldbuffer = new DX11DynamicStructuredBuffer<Matrix>(context, this.FInWorld.SliceCount); }
            if (this.colorbuffer == null) { this.colorbuffer = new DX11DynamicStructuredBuffer<Color4>(context, this.FInColor.SliceCount); }
            

            this.worldbuffer.WriteData(this.FInWorld.ToArray());
            this.colorbuffer.WriteData(this.FInColor.ToArray());

            quadshader.SetBySemantic("WORLDBUFFER", this.worldbuffer.SRV);
            quadshader.SetBySemantic("COLORBUFFER", this.colorbuffer.SRV);
            
            quadshader.SetBySemantic("WORLDCOUNT", this.worldbuffer.ElementCount);
            quadshader.SetBySemantic("COLORCOUNT", this.colorbuffer.ElementCount);
            
        }


        public void Render(IPluginIO pin, DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.spmax > 0)
            {
                if (this.FEnabled[0])
                {
                    context.CleanShaderStages();

                    quadshader.SetBySemantic("VIEWPROJECTION", settings.ViewProjection);

                    if (this.BeginQuery != null)
                    {
                        this.BeginQuery(context);
                    }

                    bool multisampler = this.FInSamplerState.SliceCount > 1 && this.FInTexture.PluginIO.IsConnected;

                    if (this.FInState.SliceCount > 1 || this.FInTexture.SliceCount > 1 || multisampler)
                    {
                        if (this.FInTexture.PluginIO.IsConnected)
                        {
                            this.RenderTextured(context);
                        }
                        else
                        {
                            this.RenderBasic(context);
                        }
                    }
                    else
                    {
                        if (this.FInTexture.PluginIO.IsConnected)
                        {
                            this.RenderInstancedTextured(context);
                        }
                        else
                        {
                            this.RenderInstanced(context);
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
