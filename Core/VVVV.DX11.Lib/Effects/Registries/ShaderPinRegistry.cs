using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Internals.Effects.Pins;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Lib.Effects.Registries
{
    

    public class ShaderPinRegistry
    {
        public delegate IShaderPin ShaderPinCreateDelegate(EffectVariable var);

        private Dictionary<string, ShaderPinCreateDelegate> delegates = new Dictionary<string, ShaderPinCreateDelegate>();

        public void RegisterType(string type, ShaderPinCreateDelegate creator)
        {
            delegates[type] = creator;
        }

        public bool ContainsType(string type)
        {
            return this.delegates.ContainsKey(type);

        }

        public IShaderPin CreatePin(string type, EffectVariable var, IPluginHost host, IIOFactory iofactory)
        {
            if (this.delegates.ContainsKey(type))
            {
                IShaderPin sp = this.delegates[type](var);
                sp.Initialize(iofactory, var);
                return sp;
            }
            else
            {
                return null;
            }
        }
    }
}
