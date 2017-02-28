using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D11;

using VVVV.DX11.Internals.Effects;
using VVVV.DX11;

using VVVV.PluginInterfaces.V2;

using FeralTic.Resources.Geometry;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using SlimDX.D3DCompiler;


namespace VVVV.DX11.Lib.Effects
{
    public class DX11ShaderData : IDX11Resource, IDisposable
    {
        private DX11RenderContext context;

        private DX11ShaderInstance shaderinstance;

        private EffectTechnique technique;
        private EffectPass pass;
        private PrimitiveTopology forcedTopology = PrimitiveTopology.Undefined;

        private DX11Effect shader;

        //Geometry/Layout Cache
        private IDX11Geometry geometryFromLayer;
        private InputLayout inputLayoutForLayerGeometry;
        private bool geometryLayoutValid;


        private List<InputLayout> layouts = new List<InputLayout>();
        private List<bool> layoutvalid = new List<bool>();
        private List<string> layoutmsg = new List<string>();

        private int passid;
        private int techid;

        //private DX11ShaderVariableManager varmanager;

        public bool IsValid { get { return this.shaderinstance != null; } }

        public List<bool> LayoutValid { get { return this.layoutvalid; } }
        public List<string> LayoutMsg { get { return this.layoutmsg; } }

        public DX11ShaderInstance ShaderInstance { get { return this.shaderinstance; } }

        public int PassCount
        {
            get
            {
                return this.technique.Description.PassCount;
            }
        }

        public DX11ShaderData(DX11RenderContext context, DX11Effect effect)
        {
            this.context = context;
            this.passid = 0;
            this.techid = 0;
            this.SetEffect(effect);
        }

        #region Set Effect
        private void SetEffect(DX11Effect shader)
        {
            //Create
            if (this.shader == null)
            {
                this.shader = shader;

                if (shader.IsCompiled)
                {
                    this.shaderinstance = new DX11ShaderInstance(this.context, shader.ByteCode);
                    this.UpdateTechnique();
                }
            }
            else
            {
                if (shader.IsCompiled)
                {
                    //Update shader
                    if (shader != this.shader)
                    {
                        //Dispose old effect if applicable
                        this.shader = shader;
                        if (this.shaderinstance != null) { this.shaderinstance.Dispose(); }
                        this.shaderinstance = new DX11ShaderInstance(this.context, shader.ByteCode);
                        this.UpdateTechnique();

                        this.DisposeLayouts();
                    }
                }
            }
        }
        #endregion

        #region Update
        public void Update(int techid, int passid, ISpread<DX11Resource<IDX11Geometry>> geoms)
        {
            this.techid = techid;
            this.passid = passid;
            this.UpdateTechnique();

            //Rebuild Layout
            this.DisposeLayouts();

            for (int i = 0; i < geoms.SliceCount; i++)
            {
                try
                {
                    if (pass.Description.Signature != null)
                    {
                        InputLayout layout = new InputLayout(this.context.Device, pass.Description.Signature, geoms[i][this.context].InputLayout);
                        this.layouts.Add(layout);
                        this.layoutvalid.Add(true);
                        this.layoutmsg.Add("OK");
                    }
                    else
                    {
                        this.layouts.Add(null);
                        this.layoutvalid.Add(true);
                        this.layoutmsg.Add("OK");
                    }
                }
                catch
                {
                    try
                    {
                        //Do bit of reflection work to get missing semantic
                        EffectShaderVariable vs = pass.VertexShaderDescription.Variable;
                        int inputcount = vs.GetShaderDescription(0).InputParameterCount;
                        string missingsemantics = "Geometry is missing semantics: ";

                        bool first = true;

                        for (int vip = 0; vip < inputcount; vip++)
                        {
                            ShaderParameterDescription sd = vs.GetInputParameterDescription(0, vip);

                            if (sd.SystemType == SystemValueType.Undefined) //Ignore SV semantics
                            {
                                bool found = false;
                                foreach (InputElement e in geoms[i][this.context].InputLayout)
                                {
                                    if (sd.SemanticName == e.SemanticName && sd.SemanticIndex == e.SemanticIndex)
                                    {
                                        found = true;
                                    }
                                }

                                if (!found)
                                {
                                    string sem = sd.SemanticIndex == 0 ? "" : sd.SemanticIndex.ToString();
                                    if (first) { first = false; } else { missingsemantics += " : "; }
                                    missingsemantics += sd.SemanticName + sem;
                                }
                            }
                        }

                        this.layouts.Add(null);
                        this.layoutvalid.Add(false);
                        this.layoutmsg.Add(missingsemantics);
                    }
                    catch (Exception ex)
                    {
                        //Just in case
                        this.layouts.Add(null);
                        this.layoutvalid.Add(false);
                        this.layoutmsg.Add(ex.Message);
                    }
                }
            }
        }
        #endregion

        #region Dispose
        public void DisposeLayouts()
        {
            for (int i = 0; i < this.layouts.Count; i++)
            {
                if (this.layouts[i] != null) { this.layouts[i].Dispose(); }
            }
            this.layouts.Clear();
            this.layoutvalid.Clear();
            this.layoutmsg.Clear();
        }

        public void Dispose()
        {
            this.DisposeLayouts();

            if (this.shaderinstance != null) { this.shaderinstance.Dispose(); }
        }
        #endregion

        #region Reset Shader Stages
        public void ResetShaderStages(DeviceContext ctx)
        {
            ctx.HullShader.Set(null);
            ctx.DomainShader.Set(null);
            ctx.VertexShader.Set(null);
            ctx.PixelShader.Set(null);
            ctx.GeometryShader.Set(null);
            ctx.ComputeShader.Set(null);
            ctx.ComputeShader.SetUnorderedAccessView(null, 0);
        }
        #endregion

        private void UpdateTechnique()
        {
            this.technique = this.shaderinstance.Effect.GetTechniqueByIndex(this.techid);
            this.pass = this.technique.GetPassByIndex(this.passid);
            this.forcedTopology = pass.Topology();
        }

        public bool IsLayoutValid(int slice)
        {
            return this.layoutvalid[slice % this.layoutvalid.Count];
        }

        public bool SetInputAssemblerFromLayer(DeviceContext ctx, IDX11Geometry geom, int slice)
        {
            if (this.geometryFromLayer != geom)
            {
                if(this.inputLayoutForLayerGeometry != null)
                {
                    this.inputLayoutForLayerGeometry.Dispose();
                    this.inputLayoutForLayerGeometry = null;
                }
            }

            this.geometryFromLayer = geom;

            if (geom == null)
            {
                geometryLayoutValid = false;
                return false;
            }

            if (this.inputLayoutForLayerGeometry == null)
            {
                try
                {
                    if (pass.Description.Signature != null)
                    {
                        this.inputLayoutForLayerGeometry = new InputLayout(this.context.Device, pass.Description.Signature, geom.InputLayout);
                        geom.Bind(this.inputLayoutForLayerGeometry);
                        if (this.forcedTopology != PrimitiveTopology.Undefined)
                        {
                            ctx.InputAssembler.PrimitiveTopology = this.forcedTopology;
                        }
                        geometryLayoutValid = true;
                        return true;
                    }
                    else
                    {
                        geometryLayoutValid = true;
                        geom.Bind(null);
                        if (this.forcedTopology != PrimitiveTopology.Undefined)
                        {
                            ctx.InputAssembler.PrimitiveTopology = this.forcedTopology;
                        }
                        geometryLayoutValid = true;
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                if (this.geometryLayoutValid)
                {
                    geom.Bind(this.inputLayoutForLayerGeometry);
                    if (this.forcedTopology != PrimitiveTopology.Undefined)
                    {
                        ctx.InputAssembler.PrimitiveTopology = this.forcedTopology;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
 
            }


        }


        public void SetInputAssembler(DeviceContext ctx, IDX11Geometry geom, int slice)
        {
            geom.Bind(this.layouts[slice % this.layouts.Count]);
            if (this.forcedTopology != PrimitiveTopology.Undefined)
            {
                ctx.InputAssembler.PrimitiveTopology = this.forcedTopology;
            }
        }

        #region Apply Pass
        public void ApplyPass(DeviceContext ctx, int passIndex= 0)
        {
            var cpass = this.technique.GetPassByIndex(passIndex);
            this.shaderinstance.ApplyPass(cpass);
        }
        #endregion
    }
}
