#pragma once

#include "FW1FontWrapper.h"
#include <map>

using namespace VVVV::PluginInterfaces::V2;
using namespace VVVV::PluginInterfaces::V1;

using namespace FeralTic::DX11;
using namespace FeralTic::DX11::Resources;

using namespace VVVV::DX11;
using namespace VVVV::Core::DirectWrite;

using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;
using namespace System;
using namespace System::ComponentModel::Composition;

namespace VVVV {
	namespace Nodes {
		namespace DX11 {

			public ref class ColorStyler : public ITextStyler
			{
			public:
				virtual void Apply(SlimDX::DirectWrite::TextLayout^ layout) { }
				SlimDX::Color4 Color;
			};

			[PluginInfo(Name = "Color", Author = "vux", Category = "DirectWrite", Version="Styles")]
			public ref class ColorStylerNode : public IPluginEvaluate
			{
			public:
				ColorStylerNode();
				virtual void Evaluate(int spreadMax) {}
			};



		}
	}
}
