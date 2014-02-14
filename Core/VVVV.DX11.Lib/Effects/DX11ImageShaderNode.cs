using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using Device = SlimDX.Direct3D11.Device;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.Hosting.Pins;

using VVVV.DX11.Internals.Effects;
using VVVV.DX11.Internals.Effects.Pins;
using VVVV.DX11.Internals;
using VVVV.DX11.Lib.Effects;
using VVVV.DX11.Lib.Devices;
using VVVV.DX11;
using VVVV.Core.Model;
using VVVV.Core.Model.FX;

using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System.CodeDom.Compiler;


namespace VVVV.DX11.Nodes.Layers
{
    public class DX11ImageShaderVariableManager : DX11ShaderVariableManager
    {
        public DX11ImageShaderVariableManager(IPluginHost host, IIOFactory iofactory) : base(host, iofactory) { }

        public List<EffectResourceVariable> texturecache = new List<EffectResourceVariable>();
        public List<ImageShaderPass> passes = new List<ImageShaderPass>();
        public List<EffectScalarVariable> passindex = new List<EffectScalarVariable>();
        

       // System.Collections.Generic.SortedDictionary<int,Vector2>

        public void RebuildTextureCache()
        {
            texturecache.Clear();
            passindex.Clear();
            for (int i = 0; i < this.shader.DefaultEffect.Description.GlobalVariableCount; i++)
            {
                EffectVariable var = this.shader.DefaultEffect.GetVariableByIndex(i);

                if (var.GetVariableType().Description.TypeName == "Texture2D")
                {
                    EffectResourceVariable rv = var.AsResource();
                    texturecache.Add(rv);
                }

                if (var.GetVariableType().Description.TypeName == "float"
                    || var.GetVariableType().Description.TypeName == "int")
                {
                    if (var.Description.Semantic == "PASSINDEX")
                    {
                        passindex.Add(var.AsScalar());
                    }
                }
            }
        }

        public void RebuildPassCache(int techidx)
        {
            passes.Clear();

            for (int i = 0; i < this.shader.DefaultEffect.GetTechniqueByIndex(techidx).Description.PassCount; i++)
            {
                ImageShaderPass pi = new ImageShaderPass(this.shader.DefaultEffect.GetTechniqueByIndex(techidx).GetPassByIndex(i));
                this.passes.Add(pi);
            }
        }
    }

    [PluginInfo(Name = "ShaderNode", Category = "DX11", Version = "", Author = "vux")]
    public unsafe class DX11ImageShaderNode : DX11BaseShaderNode,IPluginBase, IPluginEvaluate, IDisposable, IDX11ResourceProvider
    {
        private int tid = 0;

        private RenderTargetView[] nullrtvs = new RenderTargetView[8];

        private DX11ObjectRenderSettings objectsettings = new DX11ObjectRenderSettings();

        private DX11ImageShaderVariableManager varmanager;
        private Dictionary<DX11RenderContext, DX11ShaderData> deviceshaderdata = new Dictionary<DX11RenderContext, DX11ShaderData>();
        private bool shaderupdated;

        private int spmax = 0;

        private List<DX11ResourcePoolEntry<DX11RenderTarget2D>> lastframetargets = new List<DX11ResourcePoolEntry<DX11RenderTarget2D>>();
 
        #region Default Input Pins
        [Input("Depth In",Visibility=PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<DX11DepthStencil>> FDepthIn;

        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> FIn;

        [Input("Use Default Size", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<bool> FInUseDefaultSize;

        [Input("Default Size",DefaultValues= new double[] {256,256 },Visibility= PinVisibility.OnlyInspector)]
        protected ISpread<Vector2> FInSize;

        [Input("Custom Semantics", Order = 5000, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<IDX11RenderSemantic> FInSemantics;

        [Input("Resource Semantics", Order = 5001, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11RenderSemantic>> FInResSemantics;
        #endregion

        #region Output Pins

        [Output("Texture Out")]
        protected ISpread<DX11Resource<DX11Texture2D>> FOut;

        [Output("Technique Valid")]
        protected ISpread<bool> FOutTechniqueValid;
        #endregion

        #region Set the shader instance
        public override void SetShader(DX11Effect shader, bool isnew)
        {
            if (shader.IsCompiled)
            {
                this.FShader = shader;
                this.varmanager.SetShader(shader);
                this.varmanager.RebuildTextureCache();

                this.varmanager.RebuildPassCache(tid);
               //this.varmanager.RebuildPassCache(0);
            }

            //Only set technique if new, otherwise do it on update/evaluate
            if (isnew)
            {
                string defaultenum;
                if (shader.IsCompiled)
                {
                    defaultenum = shader.TechniqueNames[0];
                    this.FHost.UpdateEnum(this.TechniqueEnumId, shader.TechniqueNames[0], shader.TechniqueNames);
                    this.varmanager.CreateShaderPins();
                }
                else
                {
                    defaultenum = "";
                    this.FHost.UpdateEnum(this.TechniqueEnumId, "", new string[0]);
                }
            }
            else
            {
                if (shader.IsCompiled)
                {
                    this.FHost.UpdateEnum(this.TechniqueEnumId, shader.TechniqueNames[0], shader.TechniqueNames);
                    this.varmanager.UpdateShaderPins();
                }
            }


            this.shaderupdated = true;
            this.FInvalidate = true;
        }
        #endregion

        #region Constructor
        [ImportingConstructor()]
        public DX11ImageShaderNode(IPluginHost host, IIOFactory factory)
        {
            this.FHost = host;
            this.FFactory = factory;
            this.TechniqueEnumId = Guid.NewGuid().ToString();

            InputAttribute inAttr = new InputAttribute("Technique");
            inAttr.EnumName = this.TechniqueEnumId;
            //inAttr.DefaultEnumEntry = defaultenum;
            inAttr.Order = 1000;
            this.FInTechnique = this.FFactory.CreateDiffSpread<EnumEntry>(inAttr);

            this.varmanager = new DX11ImageShaderVariableManager(host, factory);

            
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            
            this.spmax = this.CalculateSpreadMax();

            this.FOut.SliceCount = this.spmax;
            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOut[i] == null)
                {
                    this.FOut[i] = new DX11Resource<DX11Texture2D>();
                }
            }

            if (this.FInvalidate)
            {
                if (this.FShader.IsCompiled)
                {
                    this.FOutCompiled[0] = true;
                    this.FOutTechniqueValid.SliceCount = this.FShader.TechniqueValids.Length;

                    for (int i = 0; i < this.FShader.TechniqueValids.Length; i++)
                    {
                        this.FOutTechniqueValid[i] = this.FShader.TechniqueValids[i];
                    }
                }
                else
                {
                    this.FOutCompiled[0] = false;
                    this.FOutTechniqueValid.SliceCount = 0;
                }
                this.FInvalidate = false;
            }

            if (this.FInTechnique.IsChanged)
            {
                tid = this.FInTechnique[0].Index;
                this.varmanager.RebuildPassCache(tid);
            }

            this.FOut.Stream.IsChanged = true;
        }
        #endregion

        #region Calculate Spread Max
        private int CalculateSpreadMax()
        {
            int max = this.varmanager.CalculateSpreadMax();

            if (max == 0 || this.FIn.SliceCount == 0)
            {
                return 0;
            }
            else
            {
                max = Math.Max(this.FIn.SliceCount, max);
                return max;
            }
        }
        #endregion

        #region Update
        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            Device device = context.Device;
            DeviceContext ctx = context.CurrentDeviceContext;

            if (!this.deviceshaderdata.ContainsKey(context))
            {
                this.deviceshaderdata.Add(context, new DX11ShaderData(context));
                this.deviceshaderdata[context].SetEffect(this.FShader);
            }

            DX11ShaderData shaderdata = this.deviceshaderdata[context];
            if (this.shaderupdated)
            {
                shaderdata.SetEffect(this.FShader);
                this.shaderupdated = false;
            }

            context.RenderStateStack.Push(new DX11RenderState());

            this.OnBeginQuery(context);


            //Clear shader stages
            shaderdata.ResetShaderStages(ctx);
            context.Primitives.ApplyFullTriVS();
            
            foreach (DX11ResourcePoolEntry<DX11RenderTarget2D> rt in this.lastframetargets)
            {
                rt.UnLock();
            }
            this.lastframetargets.Clear();
            
            DX11ObjectRenderSettings or = new DX11ObjectRenderSettings();

            int wi, he;

            bool preserve = false;
            DX11ResourcePoolEntry<DX11RenderTarget2D> preservedtarget = null;
            

            for (int i = 0; i < this.spmax; i++)
            {
                int passcounter = 0;

                if (this.FInEnabled[i])
                {
                    List<DX11ResourcePoolEntry<DX11RenderTarget2D>> locktargets = new List<DX11ResourcePoolEntry<DX11RenderTarget2D>>();
                    

                    #region Manage size
                    DX11Texture2D initial;
                    if (this.FIn.PluginIO.IsConnected)
                    {
                        if (this.FInUseDefaultSize[0])
                        {
                            initial = context.DefaultTextures.WhiteTexture;
                            wi = (int)this.FInSize[0].X;
                            he = (int)this.FInSize[0].Y;
                        }
                        else
                        {
                            initial = this.FIn[i][context];
                            if (initial != null)
                            {
                                wi = initial.Width;
                                he = initial.Height;
                            }
                            else
                            {
                                initial = context.DefaultTextures.WhiteTexture;
                                wi = (int)this.FInSize[0].X;
                                he = (int)this.FInSize[0].Y;
                            }
                        }
                    }
                    else
                    {
                        initial = context.DefaultTextures.WhiteTexture;
                        wi = (int)this.FInSize[0].X;
                        he = (int)this.FInSize[0].Y;
                    }
                    #endregion

                    DX11RenderSettings r = new DX11RenderSettings();
                    r.RenderWidth = wi;
                    r.RenderHeight = he;
                    if (this.FInSemantics.PluginIO.IsConnected)
                    {
                        r.CustomSemantics.AddRange(this.FInSemantics.ToArray());
                    }
                    if (this.FInResSemantics.PluginIO.IsConnected)
                    {
                        r.ResourceSemantics.AddRange(this.FInResSemantics.ToArray());
                    }

                    this.varmanager.SetGlobalSettings(shaderdata.ShaderInstance, r);

                    this.varmanager.ApplyGlobal(shaderdata.ShaderInstance);

                    DX11Texture2D lastrt = initial;
                    DX11ResourcePoolEntry<DX11RenderTarget2D> lasttmp = null;

                    List<DX11Texture2D> rtlist = new List<DX11Texture2D>();

                    //Bind Initial (once only is ok)
                    this.BindTextureSemantic(shaderdata.ShaderInstance.Effect, "INITIAL", initial);

                    //Go trough all passes
                    EffectTechnique tech = shaderdata.ShaderInstance.Effect.GetTechniqueByIndex(tid);


                    for (int j = 0; j < tech.Description.PassCount; j++)
                    {
                        ImageShaderPass pi = this.varmanager.passes[j];
                        EffectPass pass = tech.GetPassByIndex(j);

                        if (passcounter > 0)
                        {
                            int pid = j - 1;
                            string pname = "PASSRESULT" + pid;

                            this.BindTextureSemantic(shaderdata.ShaderInstance.Effect, pname, rtlist[pid]);
                        }

                        Format fmt = initial.Format;
                        if (pi.CustomFormat)
                        {
                            fmt = pi.Format;
                        }
                        bool mips = pi.Mips;

                        int w, h;
                        if (j == 0)
                        {
                            h = he;
                            w = wi;
                        }
                        else
                        {
                            h = pi.Reference == ImageShaderPass.eImageScaleReference.Initial ? he : lastrt.Height;
                            w = pi.Reference == ImageShaderPass.eImageScaleReference.Initial ? wi : lastrt.Width;
                        }

                        if (pi.DoScale)
                        {
                            h = Convert.ToInt32((float)h * pi.Scale);
                            w = Convert.ToInt32((float)w * pi.Scale);
                            h = Math.Max(h, 1);
                            w = Math.Max(w, 1);
                        }

                        //Check format support for render target, and default to rgb8 if not
                        if (!context.IsSupported(FormatSupport.RenderTarget, fmt)) 
                        { 
                            fmt = Format.R8G8B8A8_UNorm; 
                        }

                        //Since device is not capable of telling us BGR not supported
                        if (fmt == Format.B8G8R8A8_UNorm) { fmt = Format.R8G8B8A8_UNorm; }

                        DX11ResourcePoolEntry<DX11RenderTarget2D> elem;
                        if (preservedtarget != null)
                        {
                            elem = preservedtarget;
                        }
                        else
                        {
                            elem = context.ResourcePool.LockRenderTarget(w, h, fmt, new SampleDescription(1, 0), mips, 0);
                            locktargets.Add(elem);
                        }
                        DX11RenderTarget2D rt = elem.Element;


                        if (this.FDepthIn.PluginIO.IsConnected && pi.UseDepth)
                        {
                            context.RenderTargetStack.Push(this.FDepthIn[0][context], true, elem.Element);
                        }
                        else
                        {
                            context.RenderTargetStack.Push(elem.Element);
                        }

                        if (pi.Clear)
                        {
                            elem.Element.Clear(new Color4(0, 0, 0, 0));
                        }

                        #region Check for depth/blend preset
                        bool validdepth = false;
                        bool validblend = false;

                        DepthStencilStateDescription ds = new DepthStencilStateDescription();
                        BlendStateDescription bs = new BlendStateDescription();

                        if (pi.DepthPreset != "")
                        {
                            try
                            {
                                ds = DX11DepthStencilStates.Instance.GetState(pi.DepthPreset);
                                validdepth = true;
                            }
                            catch
                            {
                                
                            }
                        }

                        if (pi.BlendPreset != "")
                        {
                            try
                            {
                                bs = DX11BlendStates.Instance.GetState(pi.BlendPreset);
                                validblend = true;
                            }
                            catch
                            {

                            }
                        }
                        #endregion

                        if (validdepth || validblend)
                        {
                            DX11RenderState state = new DX11RenderState();
                            if (validdepth) { state.DepthStencil = ds; }
                            if (validblend) { state.Blend = bs; }
                            context.RenderStateStack.Push(state);
                        }

                        r.RenderWidth = w;
                        r.RenderHeight = h;
                        r.BackBuffer = elem.Element;
                        this.varmanager.ApplyGlobal(shaderdata.ShaderInstance);

                        //Apply settings (note that textures swap is handled later)
                        this.varmanager.ApplyPerObject(context, shaderdata.ShaderInstance, or, i);

                        //Bind last render target
                        this.BindTextureSemantic(shaderdata.ShaderInstance.Effect, "PREVIOUS", lastrt);

                        this.BindPassIndexSemantic(shaderdata.ShaderInstance.Effect, "PASSINDEX", j);

                        if (this.FDepthIn.PluginIO.IsConnected)
                        {
                            if (this.FDepthIn[0].Contains(context))
                            {
                                this.BindTextureSemantic(shaderdata.ShaderInstance.Effect, "DEPTHTEXTURE", this.FDepthIn[0][context]);
                            }
                        }

                        //Apply pass and draw quad
                        pass.Apply(ctx);

                        if (pi.ComputeData.Enabled)
                        {
                            pi.ComputeData.Dispatch(context, w, h);
                            context.CleanUpCS();
                        }
                        else
                        {
                            ctx.ComputeShader.Set(null);
                            context.Primitives.FullScreenTriangle.Draw();
                            ctx.OutputMerger.SetTargets(this.nullrtvs);
                        }

                        //Generate mips if applicable
                        if (pi.Mips) { ctx.GenerateMips(rt.SRV); }

                        if (!pi.KeepTarget)
                        {
                            preserve = false;
                            rtlist.Add(rt);
                            lastrt = rt;
                            lasttmp = elem;
                            preservedtarget = null;
                            passcounter++;
                        }
                        else
                        {
                            preserve = true;
                            preservedtarget = elem;
                        }


                        context.RenderTargetStack.Pop();

                        if (validblend || validdepth)
                        {
                            context.RenderStateStack.Pop();
                        }

                        if (pi.HasState)
                        {
                            context.RenderStateStack.Apply();
                        }
                    }

                    //Set last render target
                    this.FOut[i][context] = lastrt;

                    //Unlock all resources
                    foreach (DX11ResourcePoolEntry<DX11RenderTarget2D> lt in locktargets)
                    {
                        lt.UnLock();
                    }

                    //Keep lock on last rt, since don't want it overidden
                    lasttmp.Lock();

                    //this.lastframetargets.
                    //this.lasttarget = lasttmp;

                    this.lastframetargets.Add(lasttmp);
                    

                    //previousrts[context] = lasttmp.Element;
                }
                else
                {
                    this.FOut[i][context] = this.FIn[i][context];
                }
            }

            context.RenderStateStack.Pop();

            this.OnEndQuery(context);

            //UnLock previous frame in applicable
            //if (previoustarget != null) { context.ResourcePool.Unlock(previoustarget); }
        }
        
        #endregion

        #region Bind Semantics
        private void BindTextureSemantic(Effect effect,string semantic, DX11Texture2D resource)
        {
            foreach (EffectResourceVariable erv in this.varmanager.texturecache)
            {
                if (erv.Description.Semantic == semantic)
                {
                    /*erv.SetResource(resource.SRV);*/
                    effect.GetVariableByName(erv.Description.Name).AsResource().SetResource(resource.SRV);
                }
            }
        }

        private void BindPassIndexSemantic(Effect effect, string semantic,int passindex)
        {
            foreach (EffectScalarVariable erv in this.varmanager.passindex)
            {
                if (erv.Description.Semantic == semantic)
                {
                    //erv.Set(passindex);
                    effect.GetVariableByName(erv.Description.Name).AsScalar().Set(passindex);
                }
            }
        }

        private void BindSemanticSRV(Effect effect, string semantic, ShaderResourceView srv)
        {
            foreach (EffectResourceVariable erv in this.varmanager.texturecache)
            {
                if (erv.Description.Semantic == semantic)
                {
                    /*erv.SetResource(srv);*/
                    effect.GetVariableByName(erv.Description.Name).AsResource().SetResource(srv);
                }
            }
        }
        #endregion

        #region Destroy
        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            //this.FOutLayer[0].Dispose(OnDevice.Device);

            if (this.deviceshaderdata.ContainsKey(context))
            {
                this.deviceshaderdata[context].Dispose();
                this.deviceshaderdata.Remove(context);
            }

            foreach (DX11ResourcePoolEntry<DX11RenderTarget2D> entry in this.lastframetargets)
            {
                entry.UnLock();
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            foreach (DX11ShaderData sd in this.deviceshaderdata.Values)
            {
                sd.Dispose();
            }

            foreach (DX11ResourcePoolEntry<DX11RenderTarget2D> entry in this.lastframetargets)
            {
                entry.UnLock();
            }
        }
        #endregion

    }
}
