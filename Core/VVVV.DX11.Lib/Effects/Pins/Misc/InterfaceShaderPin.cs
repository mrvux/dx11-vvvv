using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.Pins.Input;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11.Lib.Effects.Pins;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    /*public class InterfaceShaderPin : AbstractShaderV2Pin<string>
    {
        public InterfaceShaderPin(EffectVariable var, IPluginHost host, IIOFactory factory) : base(var, host, factory) { }


        protected override void SetValue(DX11ShaderInstance instance, int slice)
        {
            instance.Effect.GetVariableByName(this.Name).AsInterface().ClassInstance = this.ParentEffect.GetVariableByName(this.pin[slice]).AsClassInstance();
        }

        public override bool Update(EffectVariable var)
        {
            return var.LinkClassesStr().Length == 0;
        }
    }*/

    public class RestrictedInterfaceShaderPin : AbstractShaderV2Pin<EnumEntry>
    {
        private string classes = "";
        private string eid;

        public Effect ParentEffect { get; set; }

        protected override bool RecreatePin(EffectVariable var)
        {
            if (var.LinkClassesStr().Length == 0)
            {
                return true;
            }
            else
            {
                if (var.LinkClassesStr() != this.classes)
                {
                    this.UpdateEnum(var);
                    this.classes = var.LinkClassesStr();
                }
                return false;
            }
        }

        protected override void ProcessAttribute(InputAttribute attr, EffectVariable var)
        {
            this.classes = var.LinkClassesStr();

            eid = Guid.NewGuid().ToString();
            string def = this.UpdateEnum(var);
            attr.DefaultEnumEntry = def;
            attr.EnumName = eid;
        }

        private string UpdateEnum(EffectVariable var)
        {
            string[] ename = var.LinkClasses();
            this.factory.PluginHost.UpdateEnum(eid, ename[0], ename);
            return ename[0];
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsInterface();
            return (i) => sv.ClassInstance = instance.Effect.GetVariableByName(this.pin[i].Name).AsClassInstance();
        }
}
}
