using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals;
using SlimDX;
using VVVV.DX11.Internals.Effects.Pins;
using SlimDX.Direct3D11;

using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;

namespace VVVV.DX11.Lib.Effects.RenderSemantics
{
    public abstract class AbstractRenderVariable : IRenderVariable
    {
        protected EffectVariable variable;
        public string Name { get; protected set; }
        public string Semantic { get; protected set; }
        public string TypeName { get; protected set; }
        public int Elements { get; protected set; }
        public DX11RenderContext RenderContext { get; set; }


        public AbstractRenderVariable(EffectVariable var)
        {
            this.variable = var;
            this.Name = var.Description.Name;
            this.TypeName = var.GetVariableType().Description.TypeName;
            this.Semantic = var.Description.Semantic;
            this.Elements = var.GetVariableType().Description.Elements;
        }

        public abstract void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings);


        public void Update(EffectVariable variable)
        {
            
        }

        public void Dispose()
        {
           
        }
    }

    public abstract class AbstractWorldRenderVariable : IWorldRenderVariable
    {
        public string Name { get; protected set; }
        public string Semantic { get; protected set; }
        public string TypeName { get; protected set; }
        public int Elements { get; protected set; }
        public DX11RenderContext RenderContext { get; set; }

        public AbstractWorldRenderVariable(EffectVariable var)
        {
            this.Name = var.Description.Name;
            this.TypeName = var.GetVariableType().Description.TypeName;
            this.Semantic = var.Description.Semantic;
            this.Elements = var.GetVariableType().Description.Elements;
        }

        public abstract void Apply(DX11ShaderInstance shaderinstance, DX11RenderSettings settings, DX11ObjectRenderSettings obj);


        public void Update(EffectVariable variable)
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}
