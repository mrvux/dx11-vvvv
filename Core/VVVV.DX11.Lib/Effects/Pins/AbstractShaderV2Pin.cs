using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11.Internals.Effects.Pins;
using SlimDX.Direct3D11;

namespace VVVV.DX11.Lib.Effects.Pins
{
    public abstract class AbstractShaderV2Spread<U> : AbstractShaderPin<ISpread<U>>
    {
        public override bool Constant
        {
            get { return this.pin.SliceCount == 1; }
        }

        public override int SliceCount
        {
            get { return this.pin.SliceCount; }
        }
    }

    public abstract class AbstractShaderV2Pin<U> : AbstractShaderPin<Pin<U>>
    {
        public override bool Constant
        {
            get { return this.pin.SliceCount == 1; }
        }

        public override int SliceCount
        {
            get { return this.pin.SliceCount; }
        }
    }


    public abstract class AbstractV1ColorPin : AbstractShaderPin<VVVV.PluginInterfaces.V1.IColorIn>
    {
        public override bool Constant
        {
            get { return this.pin.SliceCount == 1; }
        }

        public override int SliceCount
        {
            get { return this.pin.SliceCount; }
        }
    }
}
