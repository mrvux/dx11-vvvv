using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using FeralTic.DX11;
using VVVV.DX11.Lib.Devices;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "DevicesList", Category = "DX11", Version = "", Author = "vux", Tags = "", AutoEvaluate = true)]
    public class EnumDevicesNode : IPluginEvaluate
    {
        [Input("Refresh", IsBang = true)]
        protected ISpread<bool> FInRefresh;

        [Output("Output")]
        protected ISpread<DX11RenderContext> FOutDevices;

        [Output("Adapter Name")]
        protected ISpread<string> FOutAdapter;

        bool first = true;

        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            if (this.FInRefresh[0] || first)
            {
                List<DX11RenderContext> ctxlist = DX11GlobalDevice.DeviceManager.RenderContexts;
                this.FOutDevices.SliceCount = ctxlist.Count;
                this.FOutAdapter.SliceCount = ctxlist.Count;

                for (int i = 0; i < ctxlist.Count; i++)
                {
                    this.FOutDevices[i] = ctxlist[i];
                    try
                    {
                        this.FOutAdapter[i] = ctxlist[i].Adapter.Description.Description;
                    }
                    catch
                    {
                        this.FOutAdapter[i] = "Unknown";
                    }
                }
            }
            first = false;
        }
        #endregion
    }
}
