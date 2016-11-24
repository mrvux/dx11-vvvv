using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.Pins.Input;
using SlimDX.Direct3D11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using SlimDX;
using VVVV.Utils.VColor;
using VVVV.DX11.Lib.Effects.Pins;
using FeralTic.DX11;

namespace VVVV.DX11.Internals.Effects.Pins
{
    public unsafe class ColorShaderPin : AbstractV1ColorPin, IMultiTypeShaderPin, IUpdateShaderPin
    {
        double* colorPtr;
        int colorCnt;

        public void Update()
        {
            this.pin.GetColorPointer(out colorCnt, out colorPtr);
            colorCnt *= 4;
        }

        public override Action<int> CreateAction(DX11ShaderInstance instance)
        {
            var sv = instance.Effect.GetVariableByName(this.Name).AsVector();
            return (i) => 
            {
                Vector4 c = new Vector4((float)colorPtr[(i * 4) % colorCnt],
                    (float)colorPtr[(i * 4 + 1) % colorCnt],
                    (float)colorPtr[(i * 4 + 2) % colorCnt],
                    (float)colorPtr[(i * 4 + 3) % colorCnt]);
                sv.Set(c);
            };
        }

        public bool ChangeType(EffectVariable var)
        {
            return var.IsColor();
        }

        protected override void CreatePin(EffectVariable variable)
        {
            var visible = variable.Visible();
            this.factory.PluginHost.CreateColorInput(variable.UiName(), TSliceMode.Dynamic, visible ? TPinVisibility.True : TPinVisibility.OnlyInspector, out this.pin);

            Vector4 vec = variable.AsVector().GetVector();
            this.pin.SetSubType(new RGBAColor(vec.X, vec.Y, vec.Z, vec.W), true);
        }

        protected override void ProcessAttribute(InputAttribute attr, EffectVariable var) { }
        protected override bool RecreatePin(EffectVariable variable) { return false; }

    }
}

