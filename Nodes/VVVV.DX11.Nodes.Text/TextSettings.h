#pragma once

#include "FW1FontWrapper.h"
#include "TextFontRenderer.h"
#include <map>

using namespace FeralTic::DX11;
using namespace FeralTic::DX11::Resources;

using namespace VVVV::DX11;

using namespace System;

namespace VVVV {
	namespace Nodes {
		namespace DX11 {
			[VVVV::PluginInterfaces::V2::PluginInfo(Name = "TextSettings", Author = "vux", Category = "DX11", Version = "Advanced")]
			public ref class DX11TextSettingsNode : public VVVV::PluginInterfaces::V2::IPluginEvaluate, IDX11ResourceHost
			{
			public:
				[System::ComponentModel::Composition::ImportingConstructor()]
				DX11TextSettingsNode(SlimDX::DirectWrite::Factory^ dwFactory);
				~DX11TextSettingsNode();
				virtual void Evaluate(int SpreadMax);
				virtual void Update(DX11RenderContext^ OnDevice);
				virtual void Destroy(DX11RenderContext^ OnDevice, bool force);
			private:

				[VVVV::PluginInterfaces::V2::Input("Glyph Sheet Width", DefaultValue=512, IsSingle=true, MinValue=32)]
				VVVV::PluginInterfaces::V2::IDiffSpread<int>^ sheetSizeX;

				[VVVV::PluginInterfaces::V2::Input("Glyph Sheet Height", DefaultValue = 512, IsSingle = true, MinValue = 32)]
				VVVV::PluginInterfaces::V2::IDiffSpread<int>^ sheetSizeY;

				[VVVV::PluginInterfaces::V2::Input("Max Glyph Width", DefaultValue = 384, IsSingle = true, MinValue = 32)]
				VVVV::PluginInterfaces::V2::IDiffSpread<int>^ glyphWidth;

				[VVVV::PluginInterfaces::V2::Input("Max Glyph Height", DefaultValue = 384, IsSingle = true, MinValue = 32)]
				VVVV::PluginInterfaces::V2::IDiffSpread<int>^ glyphHeight;

				[VVVV::PluginInterfaces::V2::Input("Sheet Mip Levels", DefaultValue = 5, IsSingle = true, MinValue = 1)]
				VVVV::PluginInterfaces::V2::IDiffSpread<int>^ sheetMips;

				[VVVV::PluginInterfaces::V2::Input("Anisotropic Filtering", DefaultValue = 1, IsSingle = true)]
				VVVV::PluginInterfaces::V2::IDiffSpread<bool>^ aniso;

				[VVVV::PluginInterfaces::V2::Output("Output", IsSingle = true)]
				VVVV::PluginInterfaces::V2::ISpread<DX11Resource<TextFontRenderer^>^>^ FOutTextWrapper;

				SlimDX::DirectWrite::Factory^ dwFactory;
			};

		}
	}
}