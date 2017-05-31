using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Nodes.Text.Nodes
{
    [PluginInfo(Name = "Color", Author = "vux", Category = "DirectWrite", Version = "Styles")]
    public class ColorStylerNode : IPluginEvaluate, IDisposable
    {
        [Input("Color", DefaultColor =new double[] { 1, 1, 1, 1 })]
        public ISpread<SlimDX.Color4> color;

		[Input("From", Order = 500)]
        public IDiffSpread<int> from;

		[Input("Length", Order = 501, DefaultValue = 1)]
        public IDiffSpread<int> length;

		[Input("Enabled", Order = 502, DefaultValue = 1)]
        public IDiffSpread<bool> enabled;

		[Output("Style Out")]
        public ISpread<FWColorStyler> styleOut;

        public void Evaluate(int spreadMax)
        {
            this.styleOut.DisposeSpread();
            this.styleOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                FWColorStyler ts = new FWColorStyler();
                ts.Range.StartPosition = from[i];
                ts.Range.Length = length[i];
                ts.Enabled = enabled[i];
                ts.Color = color[i];
                this.styleOut[i] = ts;
            }
        }

        public void Dispose()
        {
            this.styleOut.DisposeSpread();
        }
    }
}
