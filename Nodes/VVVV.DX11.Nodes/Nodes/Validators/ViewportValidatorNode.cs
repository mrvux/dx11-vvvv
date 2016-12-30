using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using VVVV.PluginInterfaces.V2;

using FeralTic.Resources.Geometry;

using VVVV.DX11.Validators;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "ViewportIndex", Category = "DX11.Validator", Version = "", Author = "vux", Tags = "layer")]
    public class ViewportIndexNode : IPluginEvaluate
    {
        [Input("Enabled", DefaultValue = 1)]
        protected ISpread<bool> FInEnabled;

        [Input("Viewport Count", DefaultValue = 1, IsSingle = true)]
        protected ISpread<int> FViewportCount;

        [Input("Cyclic", DefaultValue = 0, IsSingle = true)]
        protected ISpread<bool> FCyclic;

        [Output("Output", IsSingle = true)]
        protected ISpread<DX11ViewportValidator> FOut;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOut[0] == null) { this.FOut[0] = new DX11ViewportValidator(); }
            this.FOut[0].Enabled = this.FInEnabled[0];
            this.FOut[0].ViewPortCount = this.FViewportCount[0];
            this.FOut[0].Cycling = this.FCyclic[0];
            this.FOut[0].Reset();

        }
    }


}
