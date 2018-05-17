using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.DX11.Lib.Effects;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Nodes.Layers
{
    public class DX11ImageShaderVariableManager : DX11ShaderVariableManager
    {
        public DX11ImageShaderVariableManager(IPluginHost host, IIOFactory iofactory) : base(host, iofactory) { }

        public List<EffectScalarVariable> passindex = new List<EffectScalarVariable>();
        public List<EffectScalarVariable> passiterindex = new List<EffectScalarVariable>();

        public void RebuildTextureCache()
        {
            passindex.Clear();
            for (int i = 0; i < this.shader.DefaultEffect.Description.GlobalVariableCount; i++)
            {
                EffectVariable var = this.shader.DefaultEffect.GetVariableByIndex(i);

                if (var.GetVariableType().Description.TypeName == "float"
                    || var.GetVariableType().Description.TypeName == "int")
                {
                    if (var.Description.Semantic == "PASSINDEX")
                    {
                        passindex.Add(var.AsScalar());
                    }
                    if (var.Description.Semantic == "PASSITERATIONINDEX")
                    {
                        passiterindex.Add(var.AsScalar());
                    }
                }
            }
        }
    }
}
