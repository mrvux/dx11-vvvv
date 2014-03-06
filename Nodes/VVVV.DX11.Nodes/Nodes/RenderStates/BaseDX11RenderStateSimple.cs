using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;
using VVVV.Hosting.Pins;
using FeralTic.DX11;


namespace VVVV.DX11.Nodes
{
    public abstract class BaseDX11RenderStateSimple : IPluginEvaluate
    {
        [Input("Render State", CheckIfChanged = true)]
        protected Pin<DX11RenderState> FInState;

        Pin<EnumEntry> FInPreset;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

        protected IPluginHost FHost;
        protected IIOFactory FIOFactory;

        protected abstract DX11RenderState AssignPreset(string key,DX11RenderState statein);
        protected abstract InputAttribute GetEnumPin();

        [ImportingConstructor()]
        public BaseDX11RenderStateSimple(IPluginHost host,IIOFactory iofactory)
        {
            this.FHost = host;
            this.FIOFactory = iofactory;

            InputAttribute attr = this.GetEnumPin();
            attr.CheckIfChanged = true;
            this.FInPreset = this.FIOFactory.CreatePin<EnumEntry>(attr);
        }


        public void Evaluate(int SpreadMax)
        {
            if (this.FInPreset.IsChanged
                || this.FInState.IsChanged)
            {
                this.FOutState.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    DX11RenderState rs;
                    if (this.FInState.PluginIO.IsConnected)
                    {
                        rs = this.FInState[i].Clone();
                    }
                    else
                    {
                        rs = new DX11RenderState();
                    }

                    this.AssignPreset(this.FInPreset[i].Name, rs);

                    this.FOutState[i] = rs;
                }
            }
        }

    }
}
