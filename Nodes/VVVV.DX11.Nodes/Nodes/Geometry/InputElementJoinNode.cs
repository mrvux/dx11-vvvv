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
using VVVV.DX11.Internals.Helpers;
using VVVV.Hosting.Pins.Input;
using VVVV.DX11.Lib.Devices;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes.Geometry
{
    [PluginInfo(Name = "InputElement", Category = "DX11.Geometry", Version = "", Author = "vux")]
    public class InputElementJoinNode : IPluginEvaluate
    {
        IPluginHost FHost;
        IIOFactory FIOFactory;

        [Input("Element Type",DefaultEnumEntry="Position")]
        protected IDiffSpread<eInputLayoutType> FInLayoutType;

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
            if (this.FInLayoutType.IsChanged || this.FInFormat.IsChanged)
            {
                this.FOutput.SliceCount = SpreadMax;
				int offset = 0;
                for (int i = 0; i < SpreadMax; i++)
                {
                    Format fmt= (Format)Enum.Parse(typeof(Format), this.FInFormat[i].Name);
                    this.FOutput[i] = InputLayoutFactory.GetInputElement(this.FInLayoutType[i],fmt,0,offset);
					offset += FormatHelper.FormatSizes[fmt];
                }
            }
        }
    }
}
