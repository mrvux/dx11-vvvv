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
    public delegate T ShaderPinCreateDelegate<T>(EffectVariable variable, IPluginHost host, IIOFactory iofactory);

    public class CustomVariableRegistry<T> 
    {
        
        public class CustomPin
        {
            public CustomPin(string name, string semantic,bool array, ShaderPinCreateDelegate<T> pindelegate) 
            {
                this.Delegate = pindelegate; this.Name = name; this.Semantic = semantic; this.Array = array;
            }

            public string Name { get; protected set; }
            public string Semantic { get; protected set; }
            public bool Array { get; protected set; }
            public ShaderPinCreateDelegate<T> Delegate { get; protected set; }
        }

        private List<CustomPin> custompins = new List<CustomPin>();

        public void RegisterType(string type,string semantic,bool array, ShaderPinCreateDelegate<T> creator)
        {
            CustomPin cp = new CustomPin(type, semantic,array, creator);
            custompins.Add(cp);
        }

        public bool ContainsType(string type, string semantic,bool array)
        {
            var r = from cp in custompins where cp.Name == type && cp.Semantic == semantic && cp.Array == array select cp;
            return r.Count() > 0;
        }

        public T CreatePin(string type, string semantic, bool array, EffectVariable var, IPluginHost host, IIOFactory iofactory)
        {
            foreach (CustomPin cp in this.custompins)
            {
                if (cp.Name == type && cp.Semantic == semantic && cp.Array == array)
                {
                    return cp.Delegate(var, host, iofactory);
                }
            }
            return default(T);
        }
    }
}
