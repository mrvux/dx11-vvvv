using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;
using SlimDX.Direct3D11;
using VVVV.DX11.Internals;
using SlimDX.DXGI;
using FeralTic.DX11.Utils;
using VVVV.DX11.Lib;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes.Geometry
{
    [PluginInfo(Name = "InputElement", Category = "DX11.Geometry", Version = "Join", Author = "vux")]
    public class InputElementJoinNode : IPluginEvaluate
    {
        IPluginHost FHost;
        IIOFactory FIOFactory;

        [Input("Element Type",DefaultEnumEntry="Position")]
        protected IDiffSpread<eInputLayoutType> FInLayoutType;

        [Input("Auto Index", DefaultValue = 0, Order = 1000)]
        protected IDiffSpread<bool> FAutoIndex;

        IDiffSpread<EnumEntry> FInFormat;

        [Output("Output")]
        protected ISpread<InputElement> FOutput;

        [ImportingConstructor()]
        public InputElementJoinNode(IPluginHost host,IIOFactory iofactory)
        {
            this.FHost = host;
            this.FIOFactory = iofactory;

            InputAttribute fmtAttr = new InputAttribute("Format");
            fmtAttr.EnumName = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.VertexBuffer);
            fmtAttr.DefaultEnumEntry = "R32G32B32A32_Float";
            FInFormat = this.FIOFactory.CreateDiffSpread<EnumEntry>(fmtAttr); 
        
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInLayoutType.IsChanged || this.FInFormat.IsChanged || this.FAutoIndex.IsChanged)
            {
                this.FOutput.SliceCount = SpreadMax;
				int offset = 0;

                InputElement[] elements = new InputElement[SpreadMax];

                for (int i = 0; i < SpreadMax; i++)
                {
                    Format fmt= (Format)Enum.Parse(typeof(Format), this.FInFormat[i].Name);
                    elements[i] = InputLayoutFactory.GetInputElement(this.FInLayoutType[i],fmt,0,offset);
                    offset += FormatHelper.Instance.GetSize(fmt);
                }

                if (this.FAutoIndex[0])
                {
                    InputLayoutFactory.AutoIndex(elements);
                }

                this.FOutput.AssignFrom(elements);
                
            }
        }
    }
}
