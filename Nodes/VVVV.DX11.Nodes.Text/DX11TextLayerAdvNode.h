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
using namespace SlimDX::DirectWrite;

namespace VVVV {
	namespace Nodes {
		namespace DX11 {

			[PluginInfo(Name = "Text", Author = "vux", Category = "DX11.Layer", Version = "Advanced")]
			public ref class DX11TextLayerAdvNode : public IPluginEvaluate, IDX11LayerProvider
			{
			public:
				[ImportingConstructor()]
				DX11TextLayerAdvNode(IIOFactory^ factory,SlimDX::DirectWrite::Factory^ dwFactory);
				virtual void Evaluate(int SpreadMax);
				virtual void Update(IPluginIO^ pin, DX11RenderContext^ OnDevice);
				virtual void Destroy(IPluginIO^ pin, DX11RenderContext^ OnDevice, bool force);
			private:
				ITransformIn^ FInTr;

				[Input("Text Layout", CheckIfChanged=true)]
				Pin<TextLayout^>^ FLayout;

				[Input("Color", Order = 6)]
				ISpread<SlimDX::Color4>^ FInColor;

				[Input("Enabled", IsSingle = true, DefaultValue = 1, Order = 10)]
				ISpread<bool>^ FInEnabled;

				[Output("Layer", IsSingle = true)]
				ISpread<DX11Resource<DX11Layer^>^>^ FOutLayer;

				void Render(IPluginIO^ pin, DX11RenderContext^ ctx, DX11RenderSettings^ settings);

				Dictionary<DX11RenderContext^, IntPtr>^ fontrenderers;
				int spmax;

				IIOFactory^ iofactory;

				SlimDX::DirectWrite::Factory^ dwFactory;
			};

		}
	}
}