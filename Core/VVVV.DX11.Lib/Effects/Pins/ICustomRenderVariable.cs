using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;
using VVVV.DX11.Lib.Rendering;
using SlimDX.Direct3D11;

using VVVV.DX11.Effects;

namespace VVVV.DX11.Lib.Effects
{

    public class DX11CustomRenderVariable : IDX11CustomRenderVariable
    {
        public DX11CustomRenderVariable(EffectVariable var)
        {
            this.Name = var.Description.Name;
            this.TypeName = var.GetVariableType().Description.TypeName;
            this.Semantic = var.Description.Semantic;
            var ha = var.GetAnnotationByName("help");

            if (ha != null)
            {
                this.Help = ha.AsString().GetString();
            }
            else
            {
                this.Help = "";
            }
        }

        public DX11CustomRenderVariable(string name, string typename, string semantic)
        {
            this.Name = name;
            this.TypeName = typename;
            this.Semantic = semantic;
        }

        public string Name
        {
            get;
            set;
        }

        public string TypeName
        {
            get;
            set;
        }

        public string Semantic
        {
            get;
            set;
        }

        public string Help
        {
            get;
            set;
        }
    }
}
