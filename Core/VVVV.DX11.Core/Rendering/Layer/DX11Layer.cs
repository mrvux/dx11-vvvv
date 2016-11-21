using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11
{
    public delegate void RenderDelegate<T>(DX11RenderContext context, T settings);

    public class DX11BaseLayer<T> : IDX11Resource
    {
        public RenderDelegate<T> Render;

        public bool PostUpdate
        {
            get { return true; }
        }

        public void Dispose()
        {
            
        }
    }

    /// <summary>
    /// DX11 Layer provide simple interface to tell which pin they need
    /// </summary>
    public class DX11Layer : DX11BaseLayer<DX11RenderSettings>
    {
    }

    public class DX11Shader: IDX11Resource
    {
        public DX11ShaderInstance Shader
        {
            get;
            private set;
        }

        public DX11Shader(DX11ShaderInstance instance)
        {
            this.Shader = instance;
        }

        public void ApplyShaders(DX11RenderContext context)
        {
            var vsV = Shader.CurrentTechnique.GetPassByIndex(0).VertexShaderDescription.Variable;
            if (vsV.IsValid)
            {
                context.CurrentDeviceContext.VertexShader.Set(vsV.AsShader().GetVertexShader(0));
            }

            var hsV = Shader.CurrentTechnique.GetPassByIndex(0).HullShaderDescription.Variable;
            if (hsV.IsValid)
            {
                context.CurrentDeviceContext.HullShader.Set(hsV.AsShader().GetHullShader(0));
            }

            var dsV = Shader.CurrentTechnique.GetPassByIndex(0).DomainShaderDescription.Variable;
            if (dsV.IsValid)
            {
                context.CurrentDeviceContext.DomainShader.Set(dsV.AsShader().GetDomainShader(0));
            }

            var gsV = Shader.CurrentTechnique.GetPassByIndex(0).GeometryShaderDescription.Variable;
            if (gsV.IsValid)
            {
                context.CurrentDeviceContext.GeometryShader.Set(gsV.AsShader().GetGeometryShader(0));
            }

            var psV = Shader.CurrentTechnique.GetPassByIndex(0).PixelShaderDescription.Variable;
            if (psV.IsValid)
            {
                context.CurrentDeviceContext.PixelShader.Set(psV.AsShader().GetPixelShader(0));
            }

        }

        public void Dispose()
        {
            //Owned, do nothing
        }
    }

    public class DX11Layout : IDX11Resource
    {
        public InputLayout Layout
        {
            get;
            private set;
        }

        public DX11Layout(InputLayout layout)
        {
            this.Layout = layout;
        }

        public void Dispose()
        {
            if (this.Layout != null)
            {
                this.Layout.Dispose();
                this.Layout = null;
            }
            
        }
    }
}
