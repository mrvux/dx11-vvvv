using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Nodes.Layers;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition.Hosting;

namespace VVVV.DX11.Factories
{
    [Export(typeof(IAddonFactory))]
    [Export(typeof(DX11CompImageEffectFactory))]
    [ComVisible(false)]
    public class DX11CompImageEffectFactory : AbstractDX11CompShaderFactory<DX11ImageShaderNode>
    {
        [ImportingConstructor()]
        public DX11CompImageEffectFactory(CompositionContainer parentContainer, IHDEHost hdeHost)
            : base(parentContainer,hdeHost, ".tfxc")
        {
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
            get { return "Compiled"; }
        }
    }

    [Export(typeof(IAddonFactory))]
    [Export(typeof(DX11CompEffectFactory))]
    [ComVisible(false)]
    public class DX11CompEffectFactory : AbstractDX11CompShaderFactory<DX11ShaderNode>
    {
        [ImportingConstructor()]
        public DX11CompEffectFactory(CompositionContainer parentContainer, IHDEHost hdeHost)
            : base(parentContainer, hdeHost, ".fxc")
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
            get { return "Compiled"; }
        }
    }


    [Export(typeof(IAddonFactory))]
    [Export(typeof(DX11GeomEffectFactory))]
    [ComVisible(false)]
    public class DX11GeomEffectFactory : AbstractDX11CompShaderFactory<DX11StreamOutShaderNode>
    {
        [ImportingConstructor()]
        public DX11GeomEffectFactory(CompositionContainer parentContainer, IHDEHost hdeHost)
            : base(parentContainer, hdeHost, ".gsfxc")
        {
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
            get { return "Compiled"; }
        }
    }
}
