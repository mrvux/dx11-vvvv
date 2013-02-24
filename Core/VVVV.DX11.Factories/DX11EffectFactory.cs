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
