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
    public class DX11ShaderData : IDisposable
    {
        private DX11RenderContext context;

        private DX11ShaderInstance shaderinstance;

        private EffectTechnique technique;
        private EffectPass pass;

        private DX11Effect shader;

        //Geometry/Layout Cache
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

        public DX11ShaderData(DX11RenderContext context)
        {
            this.context = context;
            this.passid = 0;
            this.techid = 0;
        }

        #region Set Effect
        public void SetEffect(DX11Effect shader)
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
        }

        public bool IsLayoutValid(int slice)
        {
            return this.layoutvalid[slice % this.layoutvalid.Count];
        }

        public void SetInputAssembler(DeviceContext ctx, IDX11Geometry geom, int slice)
        {
            geom.Bind(this.layouts[slice % this.layouts.Count]);
        }

        #region Apply Pass
        public void ApplyPass(DeviceContext ctx)
        {
            this.shaderinstance.ApplyPass(this.pass);
        }
        #endregion
    }
}
