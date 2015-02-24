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
using VVVV.DX11.Lib;
using VVVV.DX11.Lib.Rendering;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System.CodeDom.Compiler;

namespace VVVV.DX11.Nodes.Layers
{



    [PluginInfo(Name = "ShaderNode", Category = "DX11", Version = "", Author = "vux")]
    public unsafe class DX11ShaderNode : DX11BaseShaderNode, IPluginBase, IPluginEvaluate, IDisposable, IDX11LayerProvider, IPartImportsSatisfiedNotification
    {
        private DX11ObjectRenderSettings objectsettings = new DX11ObjectRenderSettings();
        

        private DX11ShaderVariableManager varmanager;
        private Dictionary<DX11RenderContext, DX11ShaderData> deviceshaderdata = new Dictionary<DX11RenderContext, DX11ShaderData>();
        private bool shaderupdated;
        private int spmax = 0;
        private bool geomconnected;
        private bool stateconnected;

        #region Default Input Pins
        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Render State", CheckIfChanged = true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Geometry", CheckIfChanged = true)]
        protected Pin<DX11Resource<IDX11Geometry>> FGeometry;

        [Input("Apply Only", Visibility=PinVisibility.OnlyInspector)]
        protected ISpread<bool> FInApplyOnly;

        protected ITransformIn FInWorld;
        protected Matrix* mworld;
        protected int mworldcount;

        //private int techniqueindex;

        #endregion

        #region Output Pins

        [Output("Layer")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        [Output("Layout Valid")]
        protected ISpread<bool> FOutLayoutValid;

        [Output("Layout Message")]
        protected ISpread<string> FOutLayoutMsg;

        [Output("Technique Valid")]
        protected ISpread<bool> FOutTechniqueValid;

        [Output("Custom Semantics", Visibility=PinVisibility.OnlyInspector)]
        protected ISpread<string> FoutCS;

        #endregion


        #region Set the shader instance

        public override void SetShader(DX11Effect shader, bool isnew)
        {
            this.FShader = shader;

            this.varmanager.SetShader(shader);

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

                //Create Technique enum pin
                InputAttribute inAttr = new InputAttribute("Technique");
                inAttr.EnumName = this.TechniqueEnumId;
                inAttr.DefaultEnumEntry = defaultenum;
                inAttr.Order = 1000;
                this.FInTechnique = this.FFactory.CreateDiffSpread<EnumEntry>(inAttr);

                this.FoutCS.AssignFrom(this.varmanager.GetCustomData());
            }
            else
            {
                if (shader.IsCompiled)
                {
                    this.FHost.UpdateEnum(this.TechniqueEnumId, shader.TechniqueNames[0], shader.TechniqueNames);
                    this.varmanager.UpdateShaderPins();
                    this.FoutCS.AssignFrom(this.varmanager.GetCustomData());
                }
            }

            this.shaderupdated = true;
            this.FInvalidate = true;
        }
        #endregion

        #region Constructor
        [ImportingConstructor()]
        public DX11ShaderNode(IPluginHost host, IIOFactory factory)
        {
            this.FHost = host;
            this.FFactory = factory;
            this.TechniqueEnumId = Guid.NewGuid().ToString();

            this.varmanager = new DX11ShaderVariableManager(host, factory);

            this.FHost.CreateTransformInput("Transform In", TSliceMode.Dynamic, TPinVisibility.True, out this.FInWorld);
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            this.shaderupdated = false;
            this.spmax = this.CalculateSpreadMax();

            if (this.FInTechnique.IsChanged)
            {
                this.techniquechanged = true;
            }
            

            float* src;

            //Cache world pointer
            this.FInWorld.GetMatrixPointer(out this.mworldcount, out src);
            this.mworld = (Matrix*)src;

            this.FOutLayer.SliceCount = 1;
            if (this.FOutLayer[0] == null)
            {
                this.FOutLayer[0] = new DX11Resource<DX11Layer>();
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
        }
        #endregion

        #region Calculate Spread Max
        private int CalculateSpreadMax()
        {
            int max = this.varmanager.CalculateSpreadMax();

            if (max == 0 || this.FInWorld.SliceCount == 0 || this.FGeometry.SliceCount == 0)
            {
                return 0;
            }
            else
            {
                max = Math.Max(this.FInWorld.SliceCount, max);
                max = Math.Max(this.FGeometry.SliceCount, max);
                return max;
            }
        }
        #endregion

        #region Update
        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (!this.FOutLayer[0].Data.ContainsKey(context))
            {
                this.FOutLayer[0][context] = new DX11Layer();
                this.FOutLayer[0][context].Render = this.Render;
            }

            if (!this.deviceshaderdata.ContainsKey(context))
            {
                this.deviceshaderdata.Add(context, new DX11ShaderData(context));
            }
            //Update shader
            this.deviceshaderdata[context].SetEffect(this.FShader);

            DX11ShaderData shaderdata = this.deviceshaderdata[context];
            if (this.shaderupdated)
            {
                shaderdata.SetEffect(this.FShader);
            }
        }
        #endregion

        #region Destroy
        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            this.FOutLayer[0].Dispose(context);

            if (this.deviceshaderdata.ContainsKey(context))
            {
                this.deviceshaderdata[context].Dispose();
                this.deviceshaderdata.Remove(context);
            }
        }
        #endregion

        #region Render
        public void Render(IPluginIO pin, DX11RenderContext context, DX11RenderSettings settings)
        {
            Device device = context.Device;
            DeviceContext ctx = context.CurrentDeviceContext;

            bool popstate = false;

            bool multistate = this.FInState.PluginIO.IsConnected && this.FInState.SliceCount > 1;

            if (this.FInEnabled[0])
            {
                if (settings.RenderHint == eRenderHint.Collector)
                {
                    if (this.FGeometry.PluginIO.IsConnected)
                    {
                        DX11ObjectGroup group = new DX11ObjectGroup();
                        group.ShaderName = this.Source.Name;
                        group.Semantics.AddRange(settings.CustomSemantics);

                        if (this.FGeometry.SliceCount == 1)
                        {
                            IDX11Geometry g = this.FGeometry[0][context];
                            if (g.Tag != null)
                            {
                                DX11RenderObject o = new DX11RenderObject();
                                o.ObjectType = g.PrimitiveType;
                                o.Descriptor = g.Tag;
                                o.Transforms = new Matrix[spmax];
                                for (int i = 0; i < this.spmax; i++)
                                {
                                    o.Transforms[i] = this.mworld[i % this.mworldcount];
                                }
                                group.RenderObjects.Add(o);

                                settings.SceneDescriptor.Groups.Add(group);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.spmax; i++)
                            {
                                IDX11Geometry g = this.FGeometry[i][context];
                                if (g.Tag != null)
                                {
                                    DX11RenderObject o = new DX11RenderObject();
                                    o.ObjectType = g.PrimitiveType;
                                    o.Descriptor = g.Tag;
                                    o.Transforms = new Matrix[1];
                                    o.Transforms[0] = this.mworld[i % this.mworldcount];
                                    group.RenderObjects.Add(o);
                                }
                            }

                            settings.SceneDescriptor.Groups.Add(group);

                        }

                    }
                    return;
                }

                DX11ShaderData shaderdata = this.deviceshaderdata[context];
                if ((shaderdata.IsValid && 
                    (this.geomconnected || settings.Geometry != null) 
                    && this.spmax > 0 && this.varmanager.SetGlobalSettings(shaderdata.ShaderInstance, settings))
                    || this.FInApplyOnly[0])
                {
                    this.OnBeginQuery(context);

                    //Need to build input layout
                    if (this.FGeometry.IsChanged || this.techniquechanged || shaderdata.LayoutValid.Count == 0)
                    {
                        shaderdata.Update(this.FInTechnique[0].Index, 0, this.FGeometry);
                        this.FOutLayoutValid.AssignFrom(shaderdata.LayoutValid);
                        this.FOutLayoutMsg.AssignFrom(shaderdata.LayoutMsg);

                        int errorCount = 0;
                        StringBuilder sbMsg = new StringBuilder();
                        sbMsg.Append("Invalid layout detected for slices:");
                        for(int i = 0; i < shaderdata.LayoutValid.Count; i++)
                        {
                            if (shaderdata.LayoutValid[i] == false)
                            {
                                errorCount++;
                                sbMsg.Append(i + ",");
                            }
                        }

                        if (errorCount > 0)
                        {
                            this.FHost.Log(TLogType.Warning, sbMsg.ToString());
                        }

                        this.techniquechanged = false;
                    }

                    if (this.stateconnected && !multistate)
                    {
                        context.RenderStateStack.Push(this.FInState[0]);
                        popstate = true;
                    }

                    if (!settings.PreserveShaderStages)
                    {
                        shaderdata.ResetShaderStages(ctx);
                    }

                    settings.DrawCallCount = spmax; //Set number of draw calls

                    this.varmanager.ApplyGlobal(shaderdata.ShaderInstance);

                    //IDX11Geometry drawgeom = null;
                    objectsettings.Geometry = null;
                    DX11Resource<IDX11Geometry> pg = null;

                    

                    for (int i = 0; i < this.spmax; i++)
                    {
                        if (multistate)
                        {
                            context.RenderStateStack.Push(this.FInState[i]);
                        }

                        if (shaderdata.IsLayoutValid(i) || settings.Geometry != null)
                        {
                            objectsettings.IterationCount = this.FIter[i];

                            for (int k = 0; k < objectsettings.IterationCount; k++)
                            {
                                objectsettings.IterationIndex = k;
                                if (settings.Geometry == null)
                                {
                                    if (this.FGeometry[i] != pg)
                                    {
                                        pg = this.FGeometry[i];

                                        objectsettings.Geometry = pg[context];

                                        shaderdata.SetInputAssembler(ctx, objectsettings.Geometry, i);
                                    }
                                }
                                else
                                {
                                    objectsettings.Geometry = settings.Geometry;
                                    shaderdata.SetInputAssembler(ctx, objectsettings.Geometry, i);
                                }

                                //Prepare settings
                                objectsettings.DrawCallIndex = i;
                                objectsettings.WorldTransform = this.mworld[i % this.mworldcount];

                                if (settings.ValidateObject(objectsettings))
                                {
                                    this.varmanager.ApplyPerObject(context, shaderdata.ShaderInstance, this.objectsettings, i);

                                    shaderdata.ApplyPass(ctx);

                                    if (settings.DepthOnly) { ctx.PixelShader.Set(null); }

                                    objectsettings.Geometry.Draw();
                                    shaderdata.ShaderInstance.CleanUp();
                                }
                            }
                        }

                        if (multistate)
                        {
                            context.RenderStateStack.Pop();
                        }
                    }

                    this.OnEndQuery(context);
                    
                }
                //this.query.End();
            }

            if (popstate)
            {
                context.RenderStateStack.Pop();
            }
            else
            {
                //Since shaders can define their own states, reapply top of the stack
                context.RenderStateStack.Apply();
            }

            if (this.FInLayer.PluginIO.IsConnected && this.FInEnabled[0])
            {
                this.FInLayer[0][context].Render(this.FInLayer.PluginIO, context, settings);
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
        }
        #endregion

        protected override void ImportsSatistieds()
        {
            this.FGeometry.Connected += new PinConnectionEventHandler(FGeometry_Connected);
            this.FGeometry.Disconnected += new PinConnectionEventHandler(FGeometry_Disconnected);
            this.FInState.Connected += new PinConnectionEventHandler(FInState_Connected);
            this.FInState.Disconnected += new PinConnectionEventHandler(FInState_Disconnected);
        }

        void FInState_Disconnected(object sender, PinConnectionEventArgs args)
        {
            this.stateconnected = false;

        }

        void FInState_Connected(object sender, PinConnectionEventArgs args)
        {
            this.stateconnected = true;
        }

        void FGeometry_Disconnected(object sender, PinConnectionEventArgs args)
        {
            this.geomconnected = false;
        }


        void FGeometry_Connected(object sender, PinConnectionEventArgs args)
        {
            this.geomconnected = true;
        }
    }
}
