using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.DX11.Lib.Devices;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "RenderOptions", Category = "DX11", Version = "Advanced.Experimental", Author = "vux", Tags = "", AutoEvaluate = true)]
    public class RenderOptionsNode : IPluginEvaluate
    {
        [Input("Disable All Rendering")]
        protected IDiffSpread<bool> FinDisableAllRendering;

        [Input("Threaded Presentation")]
        protected IDiffSpread<bool> FInThreadedPresentation;

        [Input("Thread per Device")]
        protected IDiffSpread<bool> FInThreadPerDevice;

        [Output("Thread Per Device Allowed")]
        protected ISpread<bool> FOutThreadPerDeviceAllowed;

        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            var rm = DX11GlobalDevice.RenderManager;
            rm.Enabled = !FinDisableAllRendering[0];
            rm.AllowThreadPresentation = FInThreadedPresentation[0];
            rm.AllowThreadPerDevice = FInThreadPerDevice[0];

            FOutThreadPerDeviceAllowed[0] = rm.AllowThreadPerDevice;
        }
        #endregion
    }
}
