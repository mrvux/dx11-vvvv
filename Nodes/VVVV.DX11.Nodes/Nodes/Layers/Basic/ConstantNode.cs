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
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VVVV.DX11.Nodes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct cbPerDraw
    {
        public Matrix tVP;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct cbPerObj
    {
        public Matrix tW;
	    public Matrix tTex ;
	    public Matrix tColor;
	    public Vector4 cAmb;
	    public float Alpha;
        public Vector3 dummy;
    }

    [PluginInfo(Name = "Constant", Category = "DX11.Layer", Author = "vux")]
    public class DX11ConstantLayerNode : IPluginEvaluate, IDX11LayerProvider, IDX11Queryable
    {
        [Input("Render State")]
        public Pin<DX11RenderState> FInState;

        [Input("Geometry")]
        public Pin<DX11Resource<IDX11Geometry>> FInGeometry;

        [Input("Transform In")]
        public ISpread<Matrix> FInWorld;

        [Input("Color",DefaultColor= new double[] {1,1,1,1})]
        public ISpread<Color4> FInColor;

        [Input("Alpha",DefaultValue=1)]
        public ISpread<float> FInAlpha;

        [Input("Texture In")]
        public Pin<DX11Resource<DX11Texture2D>> FInTexture;

        [Input("Color Transform")]
        public ISpread<Matrix> FInColTransform;

        [Input("Texture Transform")]
        public ISpread<Matrix> FInTexTransform;

        [Input("Samper State")]
        public Pin<SamplerDescription> FInSamplerState;

        [Input("Thread",DefaultValue=0)]
        public ISpread<bool> FInThr;

        [Input("Enabled", DefaultValue = 1, Order = 100000)]
        public IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        public int spmax;

        public DX11ShaderInstance shader;

        public Dictionary<IDX11Geometry, InputLayout> layouts = new Dictionary<IDX11Geometry, InputLayout>();

        public VertexShader vertexshader;
        public PixelShader pixelshader;
        public cbPerDraw perdraw = new cbPerDraw();
        public cbPerObj perobj = new cbPerObj();

        public DX11ConstantBuffer<cbPerDraw> cbpd;
        public DX11ConstantBuffer<cbPerObj> cbpo;
        public SamplerState sampler;

        public DeviceContext[] multicontext = new DeviceContext[8];
        public CommandList[] commands = new CommandList[8];

        public DX11RenderContext Context;

        [ImportingConstructor()]
        public DX11ConstantLayerNode(IPluginHost host, IIOFactory iofactory)
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
            this.Context = context;
            if (shader == null)
            {
                string basepath = "VVVV.DX11.Nodes.effects.constant.fx";
                DX11Effect effect = DX11Effect.FromResource(Assembly.GetExecutingAssembly(), basepath);

                shader = new DX11ShaderInstance(context, effect);

                vertexshader = shader.GetPass(0).VertexShaderDescription.Variable.GetVertexShader(0);
                pixelshader = shader.GetPass(0).PixelShaderDescription.Variable.GetPixelShader(0);
                sampler = shader.Effect.GetVariableByName("g_samLinear").AsSampler().GetSamplerState(0);
                cbpd = new DX11ConstantBuffer<cbPerDraw>(context);
                cbpo = new DX11ConstantBuffer<cbPerObj>(context);

                for (int i = 0; i < 8; i++)
                {
                    this.multicontext[i] = new DeviceContext(context.Device);
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

        static Action<DX11ConstantLayerNode,int, int, int> RenderRow = (DX11ConstantLayerNode node, int contextIndex, int fromY, int toY) =>
        {
            cbPerObj perobj = new cbPerObj();
            DeviceContext ctx = node.multicontext[contextIndex];
            
            for (int i = fromY; i < toY; i++)
            {
                IDX11Geometry geom = node.FInGeometry[i][node.Context];

                if (!node.layouts.ContainsKey(geom))
                {
                    InputLayout l;
                    geom.ValidateLayout(node.shader.GetPass(0), out l);
                    node.layouts.Add(geom, l);
                }

                geom.Bind(ctx,node.layouts[geom]);



                perobj.Alpha = node.FInAlpha[i];
                perobj.cAmb = node.FInColor[i].ToVector4();
                perobj.tColor = node.FInColTransform[i];
                perobj.tTex = node.FInTexTransform[i];
                perobj.tW = node.FInWorld[i];

                node.cbpo.Update(ctx, perobj);
                /*cbpo.Data = this.perobj;*/

                ctx.VertexShader.SetConstantBuffer(node.cbpo.Buffer, 1);
                ctx.PixelShader.SetConstantBuffer(node.cbpo.Buffer, 1);
                ctx.PixelShader.SetShaderResource(node.Context.DefaultTextures.WhiteTexture.SRV, 0);

                geom.Draw(ctx);
            }

            node.commands[contextIndex] = ctx.FinishCommandList(false);
        };

        static Action<DX11ConstantLayerNode,int> RenderDeferred = (DX11ConstantLayerNode node, int threadCount) =>
        {
            int deltaCube = node.spmax / threadCount;
            if (deltaCube == 0) deltaCube = 1;
            int nextStartingRow = 0;
            var tasks = new Task[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var threadIndex = i;
                int fromRow = nextStartingRow;
                int toRow = (i + 1) == threadCount ? node.spmax : fromRow + deltaCube;
                if (toRow > node.spmax)
                    toRow = node.spmax;
                nextStartingRow = toRow;

                tasks[i] = new Task(() => RenderRow(node,threadIndex, fromRow, toRow));
                tasks[i].Start();
            }
            Task.WaitAll(tasks);
        };


        public void Render(IPluginIO pin, DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.spmax > 0)
            {
                if (this.FEnabled[0] && this.FInGeometry.PluginIO.IsConnected)
                {
                    if (this.BeginQuery != null)
                    {
                        this.BeginQuery(context);
                    }

                    this.perdraw.tVP = settings.ViewProjection;
                    this.cbpd.Data = this.perdraw;

                    
                    context.CleanShaderStages();
                    context.CurrentDeviceContext.VertexShader.Set(this.vertexshader);
                    context.CurrentDeviceContext.PixelShader.Set(this.pixelshader);
                    context.CurrentDeviceContext.PixelShader.SetSampler(this.sampler, 0);
                    context.RenderStateStack.Push(new DX11RenderState());
                    context.CurrentDeviceContext.VertexShader.SetConstantBuffer(this.cbpd.Buffer, 0);

                    if (this.FInThr[0])
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            DeviceContext ctx = this.multicontext[i];
                            ctx.VertexShader.Set(this.vertexshader);
                            ctx.PixelShader.Set(this.pixelshader);
                            ctx.PixelShader.SetSampler(this.sampler, 0);
                            ctx.VertexShader.SetConstantBuffer(this.cbpd.Buffer, 0);
                            context.RenderTargetStack.Apply(ctx);
                        }
                       
                    }
                    else
                    {
                        context.CleanShaderStages();
                        context.CurrentDeviceContext.VertexShader.Set(this.vertexshader);
                        context.CurrentDeviceContext.PixelShader.Set(this.pixelshader);
                        context.CurrentDeviceContext.PixelShader.SetSampler(this.sampler, 0);
                        context.RenderStateStack.Push(new DX11RenderState());
                        context.CurrentDeviceContext.VertexShader.SetConstantBuffer(this.cbpd.Buffer, 0);
                    }

                    bool textconnected = this.FInTexture.PluginIO.IsConnected;

                    if (this.FInThr[0])
                    {
                        RenderDeferred(this, 8);

                        for (int i = 0; i < 8; i++)
                        {
                            var commandList = commands[i];
                            // Execute the deferred command list on the immediate context
                            context.CurrentDeviceContext.ExecuteCommandList(commandList, false);
                            commandList.Dispose();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.spmax; i++)
                        {
                            IDX11Geometry geom = this.FInGeometry[i][context];

                            if (!this.layouts.ContainsKey(geom))
                            {
                                InputLayout l;
                                geom.ValidateLayout(this.shader.GetPass(0), out l);
                                this.layouts.Add(geom, l);
                            }

                            geom.Bind(layouts[geom]);

                            this.perobj.Alpha = this.FInAlpha[i];
                            this.perobj.cAmb = this.FInColor[i].ToVector4();
                            this.perobj.tColor = this.FInColTransform[i];
                            this.perobj.tTex = this.FInTexTransform[i];
                            this.perobj.tW = this.FInWorld[i];

                            this.cbpo.Data = this.perobj;

                            context.CurrentDeviceContext.VertexShader.SetConstantBuffer(this.cbpo.Buffer, 1);
                            context.CurrentDeviceContext.PixelShader.SetConstantBuffer(this.cbpo.Buffer, 1);

                            if (textconnected && this.FInTexture[i].Contains(context))
                            {
                                context.CurrentDeviceContext.PixelShader.SetShaderResource(this.FInTexture[i][context].SRV, 0);
                            }
                            else
                            {
                                context.CurrentDeviceContext.PixelShader.SetShaderResource(context.DefaultTextures.WhiteTexture.SRV, 0);
                            }


                            geom.Draw();
                        }
                    }

                    context.RenderStateStack.Pop();

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
