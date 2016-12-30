using FeralTic.DX11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DX11.Lib.Devices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.DX11.Nodes.Nodes.Devices
{
    [PluginInfo(Name = "DisplayModes", Category = "DX11", Version = "", Author = "vux", AutoEvaluate = false)]
    public class DisplayModesNode : IPluginEvaluate
    {
        [Input("Output Index", Order = 8, DefaultValue = 0)]
        protected IDiffSpread<int> outputIndex;

        [Output("Name")]
        protected ISpread<string> name;


        [Output("Modes")]
        protected ISpread<string> info;

        public void Evaluate(int SpreadMax)
        {
            if (this.outputIndex.IsChanged)
            {
                DX11RenderContext ctx = DX11GlobalDevice.DeviceManager.RenderContexts[0];
                var ad = ctx.Adapter;

                int idx = VMath.Zmod(outputIndex[0], ad.GetOutputCount());

                var mon = ad.GetOutput(idx);

                name[0] = mon.Description.Name;

                var md = mon.GetDisplayModeList(SlimDX.DXGI.Format.R8G8B8A8_UNorm, (SlimDX.DXGI.DisplayModeEnumerationFlags)0);

                this.info.SliceCount = md.Count;

                for (int i = 0; i < md.Count; i++)
                {
                    var mode = md[i];
                    string d = String.Format("{0}x{1} @ {2}", mode.Width, mode.Height, mode.RefreshRate.Numerator / mode.RefreshRate.Denominator);
                    this.info[i] = d;
                }
            }

        }
    }
}
