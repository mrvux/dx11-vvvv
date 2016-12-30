using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Nodes.Layers;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Model;
using VVVV.Core.Model.FX;
using System.CodeDom.Compiler;
using FeralTic.DX11;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

namespace VVVV.DX11.Factories
{
    [Export(typeof(IAddonFactory))]
    [Export(typeof(DX11EffectFactory))]
    [ComVisible(false)]
    public class DX11EffectFactory : AbstractDX11ShaderFactory<DX11ShaderNode>
    {
        [ImportingConstructor()]
        public DX11EffectFactory(CompositionContainer parentContainer) : base(parentContainer,".fx")
        {
        }

        public override string JobStdSubPath
        {
            get { return "dx11"; }
        }

        protected override string NodeCategory
        {
            get { return "DX11.Effect"; }
        }

        protected override string NodeVersion
        {
            get { return ""; }
        }
    }

    [Export(typeof(IAddonFactory))]
    [Export(typeof(DX11ImageEffectFactory))]
    [ComVisible(false)]
    public class DX11ImageEffectFactory : AbstractDX11ShaderFactory<DX11ImageShaderNode>
    {
        [ImportingConstructor()]
        public DX11ImageEffectFactory(CompositionContainer parentContainer)
            : base(parentContainer, ".tfx")
        {
            DocumentFactory.RegisterLoader(".tfx", typeof(FXDocument));
        }

        public override string JobStdSubPath
        {
            get { return "texture11"; }
        }

        protected override string NodeCategory
        {
            get { return "DX11.TextureFX"; }
        }

        protected override string NodeVersion
        {
            get { return ""; }
        }

        #region Verify
        protected override List<CompilerError> VerifyShader(string file, DX11Effect effect)
        {
            List<CompilerError> errors = new List<CompilerError>();

            if (effect.DefaultEffect.Description.TechniqueCount == 0)
            {
                errors.Add(new CompilerError(file, 0, 0, "", "Effect Has No techniques"));
                return errors;
            }


            //Verify techniques
            for (int i = 0; i < effect.DefaultEffect.Description.TechniqueCount; i++)
            {
                EffectTechnique tech = effect.DefaultEffect.GetTechniqueByIndex(i);

                if (tech.Description.PassCount == 0)
                {
                    errors.Add(new CompilerError(file, 0, 0, "", "Technique: " + tech.Description.Name + " has no passes"));
                    return errors;
                }
                else
                {
                    for (int p = 0; p < tech.Description.PassCount; p++)
                    {
                        EffectPass pass = tech.GetPassByIndex(p);

                        if (!this.ComputeOrPixelOnly(pass))
                        {
                            errors.Add(new CompilerError(file, 0, 0, "", "Technique: " + tech.Description.Name + " : Pass : " + pass.Description.Name + " Must be pixel only or compute only"));
                        }
                        else
                        {
                            //Manually validate layout for pixelshader
                            if (this.PixelOnly(pass))
                            {
                                EffectShaderVariable ps = pass.PixelShaderDescription.Variable;
                                int inputcount = ps.GetShaderDescription(0).InputParameterCount;

                                bool hassvpos = false;
                                bool hasuv = false;

                                for (int ip = 0; ip < inputcount; ip++)
                                {
                                    ShaderParameterDescription sd = ps.GetInputParameterDescription(0, ip);
                                    if (sd.SystemType == SystemValueType.Position)
                                    {
                                        hassvpos = true;
                                    }
                                    if (sd.SemanticName == "TEXCOORD")
                                    {
                                        hasuv = true;
                                    }
                                }

                                if (!(hassvpos && hasuv) && inputcount == 2)
                                {
                                    errors.Add(new CompilerError(file, 0, 0, "", "Technique: " + tech.Description.Name + " : Pass : " + pass.Description.Name + " Must be SV_Position and TEXCOORD0 as input"));
                                }
                                
                            }
                        }
                    }
                }
            }

            return errors;
        }
        #endregion

        #region Check passes
        private bool ComputeOrPixelOnly(EffectPass pass)
        {
            return this.ComputeOnly(pass) || this.PixelOnly(pass);
        }

        private bool ComputeOnly(EffectPass pass)
        {
            return this.HasComputeShader(pass) && (this.HasPixelShader(pass) == false || this.HasOtherShader(pass) == false);
        }

        private bool PixelOnly(EffectPass pass)
        {
            return this.HasPixelShader(pass) && (this.HasComputeShader(pass) == false || this.HasOtherShader(pass) == false);
        }

        private bool HasComputeShader(EffectPass pass)
        {
            if (pass.ComputeShaderDescription.Variable.AsShader() != null)
            {
                return pass.ComputeShaderDescription.Variable.AsShader().GetComputeShader(0) != null;
            }
            return false;
        }

        private bool HasPixelShader(EffectPass pass)
        {
            if (pass.PixelShaderDescription.Variable.AsShader() != null)
            {
                return pass.PixelShaderDescription.Variable.AsShader().GetPixelShader(0) != null;
            }
            return false;
        }

        private bool HasOtherShader(EffectPass pass)
        {
            if (pass.VertexShaderDescription.Variable.AsShader() != null)
            {
                if (pass.VertexShaderDescription.Variable.AsShader().GetVertexShader(0) != null) { return true;}
            }

            if (pass.DomainShaderDescription.Variable.AsShader() != null)
            {
                if (pass.DomainShaderDescription.Variable.AsShader().GetDomainShader(0) != null) { return true; }
            }

            if (pass.HullShaderDescription.Variable.AsShader() != null)
            {
                if (pass.HullShaderDescription.Variable.AsShader().GetHullShader(0) != null) { return true; }
            }

            if (pass.GeometryShaderDescription.Variable.AsShader() != null)
            {
                if (pass.GeometryShaderDescription.Variable.AsShader().GetGeometryShader(0) != null) { return true; }
            }

            return false;
        }
        #endregion
    }

    [Export(typeof(IAddonFactory))]
    [Export(typeof(DX11StreamOutEffectFactory))]
    [ComVisible(false)]
    public class DX11StreamOutEffectFactory : AbstractDX11ShaderFactory<DX11StreamOutShaderNode>
    {
        [ImportingConstructor()]
        public DX11StreamOutEffectFactory(CompositionContainer parentContainer)
            : base(parentContainer, ".gsfx")
        {
            DocumentFactory.RegisterLoader(".gsfx", typeof(FXDocument));
        }

        public override string JobStdSubPath
        {
            get { return "geom11"; }
        }

        protected override string NodeCategory
        {
            get { return "DX11.GeomFX"; }
        }

        protected override string NodeVersion
        {
            get { return ""; }
        }
    }
}
