using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.DirectWrite.TextLayer
{
    public abstract class BaseTextLayoutFuncNode : IPluginEvaluate
    {
        [Input("Text Layout", CheckIfChanged = true, Order = -10)]
        protected Pin<TextLayout> FLayoutIn;

        [Input("Enabled", Order = 5000, DefaultValue=1)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Output")]
        protected ISpread<TextLayout> FLayoutOt;

        protected abstract bool IsChanged();

        protected abstract void Apply(TextLayout layout, bool enable, int slice);

        public void Evaluate(int SpreadMax)
        {
            if (this.FLayoutIn.PluginIO.IsConnected)
            {
                if (this.FLayoutIn.IsChanged || this.FEnabled.IsChanged || this.IsChanged())
                {
                    this.FLayoutOt.SliceCount = SpreadMax;

                    for (int i = 0; i < SpreadMax; i++)
                    {
                        this.Apply(this.FLayoutIn[i], this.FEnabled[i], i);
                        this.FLayoutOt[i] = this.FLayoutIn[i];
                    }

                    
                }
            }
            else
            {
                this.FLayoutOt.SliceCount = 0;
            }
        }
    }
}
