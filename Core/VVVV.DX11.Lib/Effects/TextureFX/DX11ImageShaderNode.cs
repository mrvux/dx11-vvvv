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
using VVVV.DX11.Lib.Effects;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using FeralTic.DX11.StockEffects;

namespace VVVV.DX11.Nodes.Layers
{
    [PluginInfo(Name = "ShaderNode", Category = "DX11", Version = "", Author = "vux")]
    public unsafe class DX11ImageShaderNode : DX11BaseShaderNode,IPluginBase, IPluginEvaluate, IDisposable, IDX11ResourceHost
    {
        private int spmax = 0;
        //private List<DX11ResourcePoolEntry<DX11RenderTarget2D>> lastframetargets = new List<DX11ResourcePoolEntry<DX11RenderTarget2D>>();
        private RenderTargetView[] nullrtvs = new RenderTargetView[8];

        private DX11ObjectRenderSettings objectsettings = new DX11ObjectRenderSettings();
        private DX11ImageShaderVariableManager varmanager;
        private DX11ContextElement<DX11ShaderVariableCache> shaderVariableCache = new DX11ContextElement<DX11ShaderVariableCache>();
        private DX11ContextElement<DX11ShaderData> deviceshaderdata = new DX11ContextElement<DX11ShaderData>();
        private DX11ContextElement<ImageShaderInfo> imageShaderInfo = new DX11ContextElement<ImageShaderInfo>();

        private Spread<DX11ResourcePoolEntry<DX11RenderTarget2D>> previousFrameResults = new Spread<DX11ResourcePoolEntry<DX11RenderTarget2D>>();

        #region Default Input Pins
        [Input("Depth In",Visibility=PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<DX11DepthStencil>> FDepthIn;

        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> FIn;

        [Input("Use Default Size", DefaultValue = 0, Visibility = PinVisibility.Hidden)]
        protected ISpread<bool> FInUseDefaultSize;

        [Input("Default Size",DefaultValues= new double[] {256,256 },Visibility= PinVisibility.Hidden)]
        protected ISpread<Vector2> FInSize;

        [Input("Preserve On Disable", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<bool> FInPreserveOnDisable;

        [Input("Mips On Last Pass", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<bool> FInMipLastPass;

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
        public override void SetShader(DX11Effect shader, bool isnew, string fileName)
        {
            FOutPath.SliceCount = 1;
            FOutPath[0] = fileName;

            if (shader.IsCompiled)
            {
                this.FShader = shader;
                this.varmanager.SetShader(shader);
                this.varmanager.RebuildTextureCache();

                this.shaderVariableCache.Clear();
                this.imageShaderInfo.Clear();
                this.deviceshaderdata.Dispose();
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

            this.previousFrameResults.Resize(this.spmax, () => null, rt => rt?.UnLock());


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

            this.varmanager.ApplyUpdates();

            this.FOut.Stream.IsChanged = true;
        }
        #endregion

        #region Calculate Spread Max
        private int CalculateSpreadMax()
        {
            int max = this.varmanager.CalculateSpreadMax();

            int spFixed = SpreadUtils.SpreadMax(this.FIn, this.FInTechnique);
            if (max == 0 || spFixed == 0)
            {
                return 0;
            }
            else
            {
                
                max = Math.Max(spFixed, max);
                return max;
            }
        }
        #endregion

        #region Update
        public void Update(DX11RenderContext context)
        {
            Device device = context.Device;
            DeviceContext ctx = context.CurrentDeviceContext;

            if (!this.deviceshaderdata.Contains(context))
            {
                this.deviceshaderdata[context] = new DX11ShaderData(context, this.FShader);
            }
            if (!this.shaderVariableCache.Contains(context))
            {
                this.shaderVariableCache[context] = new DX11ShaderVariableCache(context, this.deviceshaderdata[context].ShaderInstance, this.varmanager);
            }
            if (!this.imageShaderInfo.Contains(context))
            {
                this.imageShaderInfo[context] = new ImageShaderInfo(this.deviceshaderdata[context].ShaderInstance);
            }

            DX11ShaderData shaderdata = this.deviceshaderdata[context];
            ImageShaderInfo shaderInfo = this.imageShaderInfo[context];
            context.RenderStateStack.Push(new DX11RenderState());

            this.OnBeginQuery(context);


            //Clear shader stages
            shaderdata.ResetShaderStages(ctx);
            context.Primitives.ApplyFullTriVS();

            for (int i = 0; i < this.previousFrameResults.SliceCount; i++)
            {
                if (this.FInEnabled[i] || this.FInPreserveOnDisable[i] == false)
                {
                    this.previousFrameResults[i]?.UnLock();
                    this.previousFrameResults[i] = null;
                }
            }

            DX11ObjectRenderSettings or = new DX11ObjectRenderSettings();

            int wi, he;
            DX11ResourcePoolEntry<DX11RenderTarget2D> preservedtarget = null;
            
            for (int textureIndex = 0; textureIndex < this.spmax; textureIndex++)
            {
                int passcounter = 0;

                if (this.FInEnabled[textureIndex])
                {
                    List<DX11ResourcePoolEntry<DX11RenderTarget2D>> locktargets = new List<DX11ResourcePoolEntry<DX11RenderTarget2D>>();

                    #region Manage size
                    DX11Texture2D initial;
                    if (this.FIn.IsConnected)
                    {
                        if (this.FInUseDefaultSize[0])
                        {
                            if (this.FIn[textureIndex].Contains(context) && this.FIn[textureIndex][context] != null)
                            {
                                initial = this.FIn[textureIndex][context];
                            }
                            else
                            {
                                initial = context.DefaultTextures.WhiteTexture;
                            }
                            wi = (int)this.FInSize[0].X;
                            he = (int)this.FInSize[0].Y;
                        }
                        else
                        {
                            initial = this.FIn[textureIndex].Contains(context) ? this.FIn[textureIndex][context] : null;
                            if (initial != null)
                            {
                                wi = initial.Width;
                                he = initial.Height;
                            }
                            else
                            {
                                initial = context.DefaultTextures.WhiteTexture;
                                wi = (int)this.FInSize[textureIndex].X;
                                he = (int)this.FInSize[textureIndex].Y;
                            }
                        }
                    }
                    else
                    {
                        initial = context.DefaultTextures.WhiteTexture;
                        wi = (int)this.FInSize[textureIndex].X;
                        he = (int)this.FInSize[textureIndex].Y;
                    }
                    #endregion

                    DX11RenderSettings r = new DX11RenderSettings();
                    r.RenderWidth = wi;
                    r.RenderHeight = he;
                    if (this.FInSemantics.IsConnected)
                    {
                        r.CustomSemantics.AddRange(this.FInSemantics.ToArray());
                    }
                    if (this.FInResSemantics.IsConnected)
                    {
                        r.ResourceSemantics.AddRange(this.FInResSemantics.ToArray());
                    }

                    this.varmanager.SetGlobalSettings(shaderdata.ShaderInstance, r);
                    var variableCache = this.shaderVariableCache[context];
                    variableCache.ApplyGlobals(r);

                    
                    DX11ResourcePoolEntry<DX11RenderTarget2D> lasttmp = null;

                    List<DX11Texture2D> rtlist = new List<DX11Texture2D>();

                    //Go trough all passes
                    int tid = this.FInTechnique[textureIndex].Index;
                    ImageShaderTechniqueInfo techniqueInfo = shaderInfo.GetTechniqueInfo(tid);

                    //Now we need to add optional extra pass in case we want mip chain (only in case it's not needed, if texture has mips we just ignore)
                    if (techniqueInfo.WantMips)
                    {
                        //Single level and bigger than 1 should get a mip generation pass
                        if (initial.Width > 1 && initial.Height > 1 && initial.Resource.Description.MipLevels == 1)
                        {
                            //Texture might now be an allowed render target format, so we at least need to check that, and default to rgba8 unorm
                            //also check for auto mip map gen
                            var mipTargetFmt = initial.Format;
                            if (!context.IsSupported(FormatSupport.RenderTarget, mipTargetFmt) ||
                                !context.IsSupported(FormatSupport.MipMapAutoGeneration, mipTargetFmt) ||
                                !context.IsSupported(FormatSupport.UnorderedAccessView, mipTargetFmt))
                            {
                                mipTargetFmt = Format.R8G8B8A8_UNorm;
                            }



                            DX11ResourcePoolEntry<DX11RenderTarget2D> mipTarget = context.ResourcePool.LockRenderTarget(initial.Width, initial.Height, mipTargetFmt, new SampleDescription(1, 0), true, 0);
                            locktargets.Add(mipTarget);

                            context.RenderTargetStack.Push(mipTarget.Element);

                            context.BasicEffects.PointSamplerPixelPass.Apply(initial.SRV);

                            context.CurrentDeviceContext.Draw(3, 0);

                            context.RenderTargetStack.Pop();

                            context.CurrentDeviceContext.GenerateMips(mipTarget.Element.SRV);

                            //Replace initial by our new texture
                            initial = mipTarget.Element;
                        }   

                    }

                    //Bind Initial (once only is ok) and mark for previous usage too
                    DX11Texture2D lastrt = initial;
                    shaderInfo.ApplyInitial(initial.SRV);

                    for (int passIndex = 0; passIndex < techniqueInfo.PassCount; passIndex++)
                    {
                        ImageShaderPassInfo passInfo = techniqueInfo.GetPassInfo(passIndex);
                        bool isLastPass = passIndex == techniqueInfo.PassCount - 1;

                        for (int kiter = 0; kiter < passInfo.IterationCount; kiter++)
                        {
                            Format fmt = initial.Format;
                            if (passInfo.CustomFormat)
                            {
                                fmt = passInfo.Format;
                            }
                            bool mips = passInfo.Mips || (isLastPass && FInMipLastPass[textureIndex]);

                            int w, h;
                            if (passIndex == 0)
                            {
                                h = he;
                                w = wi;
                            }
                            else
                            {
                                h = passInfo.Reference == ImageShaderPassInfo.eImageScaleReference.Initial ? he : lastrt.Height;
                                w = passInfo.Reference == ImageShaderPassInfo.eImageScaleReference.Initial ? wi : lastrt.Width;
                            }

                            if (passInfo.DoScale)
                            {
                                if (passInfo.Absolute)
                                {
                                    w = Convert.ToInt32(passInfo.ScaleVector.X);
                                    h = Convert.ToInt32(passInfo.ScaleVector.Y);
                                }
                                else
                                {
                                    w = Convert.ToInt32((float)w * passInfo.ScaleVector.X);
                                    h = Convert.ToInt32((float)h * passInfo.ScaleVector.Y);
                                }

                                w = Math.Max(w, 1);
                                h = Math.Max(h, 1);
                            }

                            //Check format support for render target, and default to rgb8 if not
                            if (!context.IsSupported(FormatSupport.RenderTarget, fmt))
                            {
                                fmt = Format.R8G8B8A8_UNorm;
                            }

                            //To avoid uav issue
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


                            if (this.FDepthIn.IsConnected && passInfo.UseDepth)
                            {
                                context.RenderTargetStack.Push(this.FDepthIn[0][context], true, elem.Element);
                            }
                            else
                            {
                                context.RenderTargetStack.Push(elem.Element);
                            }

                            if (passInfo.Clear)
                            {
                                elem.Element.Clear(new Color4(0, 0, 0, 0));
                            }

                            #region Check for depth/blend preset
                            bool validdepth = false;
                            bool validblend = false;

                            DepthStencilStateDescription ds = new DepthStencilStateDescription();
                            BlendStateDescription bs = new BlendStateDescription();

                            if (passInfo.DepthPreset != "")
                            {
                                try
                                {
                                    ds = DX11DepthStencilStates.GetState(passInfo.DepthPreset);
                                    validdepth = true;
                                }
                                catch
                                {

                                }
                            }

                            if (passInfo.BlendPreset != "")
                            {
                                try
                                {
                                    bs = DX11BlendStates.GetState(passInfo.BlendPreset);
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

                            //Apply settings (we do both here, as texture size semantic might ahve 
                            variableCache.ApplyGlobals(r);
                            variableCache.ApplySlice(or, textureIndex);
                            //Bind last render target

                            shaderInfo.ApplyPrevious(lastrt.SRV);

                            this.BindPassIndexSemantic(shaderdata.ShaderInstance.Effect, passIndex);
                            this.BindPassIterIndexSemantic(shaderdata.ShaderInstance.Effect, kiter);

                            if (this.FDepthIn.IsConnected)
                            {
                                if (this.FDepthIn[0].Contains(context))
                                {
                                    shaderInfo.ApplyDepth(this.FDepthIn[0][context].SRV);
                                }
                            }

                            //Apply pass and draw quad
                            passInfo.Apply(ctx);

                            if (passInfo.ComputeData.Enabled)
                            {
                                passInfo.ComputeData.Dispatch(context, w, h);
                                context.CleanUpCS();
                            }
                            else
                            {
                                ctx.ComputeShader.Set(null);
                                context.Primitives.FullScreenTriangle.Draw();
                                ctx.OutputMerger.SetTargets(this.nullrtvs);
                            }

                            //Generate mips if applicable
                            if (mips) { ctx.GenerateMips(rt.SRV); }

                            if (!passInfo.KeepTarget)
                            {
                                rtlist.Add(rt);
                                lastrt = rt;
                                lasttmp = elem;
                                preservedtarget = null;
                                passcounter++;
                            }
                            else
                            {
                                preservedtarget = elem;
                            }


                            context.RenderTargetStack.Pop();

                            //Apply pass result semantic if applicable (after pop)
                            shaderInfo.ApplyPassResult(lasttmp.Element.SRV, passIndex);

                            if (validblend || validdepth)
                            {
                                context.RenderStateStack.Pop();
                            }

                            if (passInfo.HasState)
                            {
                                context.RenderStateStack.Apply();
                            }

                            

                        }
                    }

                    //Set last render target
                    this.FOut[textureIndex][context] = lastrt;

                    //Unlock all resources
                    foreach (DX11ResourcePoolEntry<DX11RenderTarget2D> lt in locktargets)
                    {
                        lt.UnLock();
                    }

                    //Keep lock on last rt, since don't want it overidden
                    lasttmp.Lock();

                    this.previousFrameResults[textureIndex] = lasttmp;
                }
                else
                {
                    if (this.FInPreserveOnDisable[textureIndex])
                    {
                        //We kept it locked on top
                        this.FOut[textureIndex][context] = this.previousFrameResults[textureIndex] != null ? this.previousFrameResults[textureIndex].Element : null;
                    }
                    else
                    {
                        this.FOut[textureIndex][context] = this.FIn[textureIndex][context];
                    }
                    
                }
            }

            context.RenderStateStack.Pop();

            this.OnEndQuery(context);
        }
        
        #endregion

        #region Bind Semantics
        private void BindPassIndexSemantic(Effect effect,int passindex)
        {
            foreach (EffectScalarVariable erv in this.varmanager.passindex)
            {
                effect.GetVariableByName(erv.Description.Name).AsScalar().Set(passindex);
            }
        }

        private void BindPassIterIndexSemantic(Effect effect, int passiterindex)
        {
            foreach (EffectScalarVariable erv in this.varmanager.passiterindex)
            {
                effect.GetVariableByName(erv.Description.Name).AsScalar().Set(passiterindex);
            }
        }

        #endregion

        #region Destroy
        public void Destroy(DX11RenderContext context, bool force)
        {
            if (force)
            {
                this.deviceshaderdata.Dispose(context);
                this.shaderVariableCache.Dispose(context);
            }
            foreach (DX11ResourcePoolEntry<DX11RenderTarget2D> entry in this.previousFrameResults)
            {
                if (entry != null)
                {
                    entry.UnLock();
                }
            }
            this.previousFrameResults.SliceCount = 0;
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            this.deviceshaderdata.Dispose();
            this.shaderVariableCache.Dispose();

            foreach (DX11ResourcePoolEntry<DX11RenderTarget2D> entry in this.previousFrameResults)
            {
                if (entry != null)
                {
                    entry.UnLock();
                }
            }
            this.previousFrameResults.SliceCount = 0;
        }
        #endregion

    }
}
