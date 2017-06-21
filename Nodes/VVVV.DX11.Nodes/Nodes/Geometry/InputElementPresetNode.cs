using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using FeralTic.DX11.Utils;

namespace VVVV.DX11.Nodes.Geometry
{
    [PluginInfo(Name = "InputElement", Category = "DX11.Geometry", Version = "Preset", Author = "vux")]
    public class InputElementPresetNode : IPluginEvaluate
    {
        [Input("Preset Name",EnumName = VertexLayoutsHelpers.VertexLayoutsEnumName, DefaultEnumEntry ="Pos3Norm3Tex2")]
        protected IDiffSpread<EnumEntry> FInLayoutType;

        [Output("Output")]
        protected ISpread<ISpread<InputElement>> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInLayoutType.IsChanged)
            {
                this.FOutput.SliceCount = this.FInLayoutType.SliceCount;
                for (int i = 0; i < this.FInLayoutType.SliceCount; i++)
                {
                    InputElement[] elements = VertexLayoutsHelpers.Elements[FInLayoutType[i]];

                    this.FOutput[i].AssignFrom(elements);
                }
            }
        }
    }
}
