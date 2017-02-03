using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.Pins.Input;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins;
using VVVV.Hosting.IO.Pointers;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public abstract class AbstractShaderPin<T> : IShaderPin where T : class
    {
        protected T pin;
        protected IIOContainer<T> container;
        protected IIOFactory factory;

        private bool visible;

        public string PinName { get; private set; }
        public string Name { get; private set; }
        public int Elements { get; private set; }
        public string TypeName { get; private set; }

        public void Initialize(IIOFactory factory, EffectVariable variable)
        {
            this.factory = factory;
            this.PinName = variable.UiName();
            this.visible = variable.Visible();
            this.TypeName = variable.GetVariableType().Description.TypeName;
            this.Elements = variable.GetVariableType().Description.Elements;
            this.Name = variable.Description.Name;

            this.CreatePin(variable);
        }

        public void Update(EffectVariable variable)
        {
            bool rebuild = variable.UiName() != this.PinName || variable.Visible() != this.visible;

            if (!rebuild)
            {
                //If name changed, recreate in any case
                rebuild = this.RecreatePin(variable);
            }

            if (rebuild)
            {
                this.container.Dispose();

                this.CreatePin(variable);
            }
        }

        protected virtual void CreatePin(EffectVariable variable)
        {
            this.visible = variable.Visible();

            InputAttribute attr = new InputAttribute(this.PinName);
            attr.Visibility = this.visible ? PinVisibility.True : PinVisibility.OnlyInspector;
            this.ProcessAttribute(attr, variable);

            this.container = factory.CreateIOContainer<T>(attr);
            this.pin = this.container.IOObject;
        }

        protected abstract void ProcessAttribute(InputAttribute attr, EffectVariable var);
        protected abstract bool RecreatePin(EffectVariable variable);

        public abstract bool Constant { get; }
        public abstract int SliceCount { get; }
        public abstract Action<int> CreateAction(DX11ShaderInstance instance);

        public void Dispose()
        {
            if (this.container != null)
            {
                this.container.Dispose();
            }
        }
    }

}
