#pragma once

#include "FW1FontWrapper.h"
#include <map>

using namespace VVVV::PluginInterfaces::V2;
using namespace VVVV::PluginInterfaces::V1;

using namespace FeralTic::DX11;
using namespace FeralTic::DX11::Resources;

using namespace VVVV::DX11;

using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;
using namespace System;
using namespace System::ComponentModel::Composition;

namespace VVVV { namespace Nodes { namespace DX11 {

[PluginInfo(Name="Text",Author="vux",Category="DX11.Layer",Version="")]
public ref class DX11TextLayerNode : public IPluginEvaluate,IDX11LayerProvider
{
public:
	[ImportingConstructor()]
	DX11TextLayerNode(IIOFactory^ factory);
	virtual void Evaluate(int SpreadMax);
	virtual void Update(IPluginIO^ pin, DX11RenderContext^ OnDevice);
	virtual void Destroy(IPluginIO^ pin, DX11RenderContext^ OnDevice, bool force);
private:
	ITransformIn^ FInTr;

	[Input("String",DefaultString="DX11", Order=0)]
    ISpread<System::String^>^ FInString;

	[Input("Font", EnumName = "SystemFonts", Order = 2)]
	ISpread<EnumEntry^>^ FFontInput;

	[Input("Italic", Order = 3)]
	ISpread<bool>^ FItalicInput;

	[Input("Bold", Order = 4)]
	IDiffSpread<bool>^ FBoldInput;

	[Input("Size", Order = 5)]
    ISpread<float>^ FInSize;

	[Input("Color", Order = 6)]
    ISpread<SlimDX::Color4>^ FInColor;

	[Input("Horizontal Align", EnumName = "HorizontalAlign", Order = 7)]
	ISpread<EnumEntry^>^ FHorizontalAlignInput;

	[Input("Vertical Align", EnumName = "VerticalAlign", Order = 8)]
	ISpread<EnumEntry^>^ FVerticalAlignInput;

	[Input("Normalize", EnumName = "Normalize", Order = 9)]
	ISpread<EnumEntry^>^ FNormalizeInput;

	[Input("Enabled", IsSingle = true, DefaultValue = 1, Order = 10)]
	ISpread<bool>^ FInEnabled;


	[Output("Layer", IsSingle=true)]
    ISpread<DX11Resource<DX11Layer^>^>^ FOutLayer;

	void Render(IPluginIO^ pin,DX11RenderContext^ ctx, DX11RenderSettings^ settings);

	Dictionary<DX11RenderContext^,IntPtr>^ fontrenderers;
	int spmax;

	IIOFactory^ iofactory;
};

}}}