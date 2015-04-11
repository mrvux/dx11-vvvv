using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;

namespace VVVV.DX11.Lib.Effects.Pins
{
    public abstract class AbstractValuePin<U> : AbstractShaderV2Pin<U>
    {
        private double uimin;
        private double uimax;
        private double uistep;

        protected virtual double DefaultStep { get { return 0.01; } }

        protected abstract void SetDefault(InputAttribute attr, EffectVariable var);

        protected override void ProcessAttribute(InputAttribute attr, EffectVariable var)
        {
            this.uimin = var.UiMin();
            this.uimax = var.UiMax();
            this.uistep = var.UiStep();

            attr.MinValue = this.uimin;
            attr.MaxValue = this.uimax;
            attr.StepSize = this.uistep == -1 ? this.DefaultStep : this.uistep;
            this.SetDefault(attr, var);
        }

        protected override bool RecreatePin(EffectVariable var)
        {
            return var.UiMin() != this.uimin || var.UiMax() != this.uimax || var.UiStep() != this.uistep;
        }
    }
}
