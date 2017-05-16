using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace VVVV.DX11.Nodes.Geometry
{
    [PluginInfo(Name = "InputElement", Category = "DX11.Geometry", Version = "Split", Author = "vux")]
    public class InputElementSplitNode : IPluginEvaluate
    {
        [Input("Input")]
        protected Pin<InputElement> FInput;

        [Output("Slot")]
        protected ISpread<int> FOutSlot;

        [Output("Semantic Name")]
        protected ISpread<string> FOutSemanticName;

        [Output("Semantic Index")]
        protected ISpread<int> FOutSemanticIndex;

        [Output("Format")]
        protected ISpread<Format> FOutFormat;

        [Output("Offset")]
        protected ISpread<int> FOutOffset;

        [Output("Per Instance")]
        protected ISpread<bool> FOutPerInstance;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInput.PluginIO.IsConnected)
            {
                this.FOutSemanticName.SliceCount = SpreadMax;
                this.FOutSemanticIndex.SliceCount = SpreadMax;
                this.FOutOffset.SliceCount = SpreadMax;
                this.FOutPerInstance.SliceCount = SpreadMax;
                this.FOutSlot.SliceCount = SpreadMax;
                this.FOutFormat.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    InputElement elem = this.FInput[i];

                    this.FOutSemanticName[i] = elem.SemanticName;
                    this.FOutSemanticIndex[i] = elem.SemanticIndex;
                    this.FOutOffset[i] = elem.AlignedByteOffset;
                    this.FOutPerInstance[i] = elem.Classification == InputClassification.PerInstanceData;
                    this.FOutSlot[i] = elem.Slot;
                    this.FOutFormat[i] = elem.Format;
                }
            }
            else
            {
                this.FOutSemanticName.SliceCount = 0;
                this.FOutSemanticIndex.SliceCount = 0;
                this.FOutOffset.SliceCount = 0;
                this.FOutPerInstance.SliceCount = 0;
                this.FOutSlot.SliceCount = 0;
                this.FOutFormat.SliceCount = 0;

            }
        }
    }
}
