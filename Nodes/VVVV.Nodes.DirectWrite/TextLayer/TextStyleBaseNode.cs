using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Core.DirectWrite;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    public abstract class TextStyleBaseNode : IPluginEvaluate
    {
        protected abstract class TextStyleBase : ITextStyler
        {
            public TextRange Range;
            public bool Enabled;

            protected abstract void DoApply(TextLayout layout, TextRange range);

            public void Apply(TextLayout layout)
            {
                if (this.Enabled)
                {
                    this.DoApply(layout, this.Range);
                }
            }
        }

        [Input("From",  Order=500)]
        protected IDiffSpread<int> from;

        [Input("Length", Order = 501, DefaultValue=1)]
        protected IDiffSpread<int> length;

        [Input("Enabled", Order = 502, DefaultValue=1)]
        protected IDiffSpread<bool> enabled;

        [Output("Style Out")]
        protected ISpread<ITextStyler> styleOut;

        protected abstract TextStyleBase CreateStyle(int slice);

        public void Evaluate(int SpreadMax)
        {
            this.styleOut.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
             {
                 TextStyleBase ts = this.CreateStyle(i);
                 ts.Range = new TextRange(from[i], length[i]);
                 ts.Enabled = this.enabled[i];
                 this.styleOut[i] = ts;
             }
        }
    }
}
