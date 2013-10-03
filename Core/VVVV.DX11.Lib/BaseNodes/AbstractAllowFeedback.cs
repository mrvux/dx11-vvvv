using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins;
using VVVV.DX11.Lib.Devices;
using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Nodes
{
    public class AllowFeedback<T> : IPluginEvaluate
    {
        [Input("Input")]
        protected Pin<T> FInput;

        [Output("Output", AllowFeedback = true)]
        protected ISpread<T> FOutput;

        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
                FOutput[i] = FInput[i];
        }

    }
}

