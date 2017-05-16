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
    [PluginInfo(Name = "InputElement", Category = "DX11.Geometry", Version = "Join Advanced", Author = "vux")]
    public class InputElementJoinAdvancedNode : IPluginEvaluate
    {
        [Input("Name",DefaultString="POSITION")]
        protected IDiffSpread<string> FInName;

        [Input("Index", DefaultValue=0)]
        protected IDiffSpread<int> FInIndex;

        [Input("Element Type", DefaultEnumEntry = "R32G32B32A32_Float")]
        protected IDiffSpread<SlimDX.DXGI.Format> FInFormat;


        [Input("Offset", DefaultValue = -1)]
        protected IDiffSpread<int> FInOffset;

        [Input("Slot", DefaultValue = 0)]
        protected IDiffSpread<int> FInSlot;

        [Input("Per Vertex", DefaultValue = 1)]
        protected IDiffSpread<bool> FInPerVertex;

        [Input("Step Rate", DefaultValue = 1)]
        protected IDiffSpread<int> FInStepRate;

        [Output("Output")]
        protected ISpread<InputElement> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInName.IsChanged || this.FInIndex.IsChanged || this.FInFormat.IsChanged
                || this.FInSlot.IsChanged || this.FInStepRate.IsChanged || this.FInPerVertex.IsChanged)
            {
                this.FOutput.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FOutput[i] = new InputElement(
                        this.FInName[i],
                        this.FInIndex[i],
                        this.FInFormat[i],
                        this.FInOffset[i],
                        this.FInSlot[i],
                        this.FInPerVertex[i] ? InputClassification.PerVertexData : InputClassification.PerInstanceData,
                        this.FInStepRate[i]
                        );
                }
            }
        }
    }
}
