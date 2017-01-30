#pragma once

#include "FW1FontWrapper.h"
#include "TextFontRenderer.h"
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
using namespace VVVV::DX11;

namespace VVVV {
	namespace Nodes {
		namespace DX11 {

			[PluginInfo(Name = "DrawTextCache", Author = "vux", Category = "DX11.Layer", Version = "Advanced")]
			public ref class DX11TextLayerCacheNode : public IPluginEvaluate, IDX11LayerHost
			{
			public:
				[ImportingConstructor()]
				DX11TextLayerCacheNode(SlimDX::DirectWrite::Factory^ dwFactory);
				virtual void Evaluate(int SpreadMax);
				virtual void Update(DX11RenderContext^ OnDevice);
				virtual void Destroy(DX11RenderContext^ OnDevice, bool force);
			private:
				[Input("Text Renderer", Visibility = PinVisibility::OnlyInspector)]
				Pin<DX11Resource<TextFontRenderer^>^>^ FTextRenderer;

				[Input("Text Format", AutoValidate = false)]
				Pin<TextFormat^>^ textFormat;

				[Input("Text Objects", AutoValidate=false)]
				Pin<TextObject^>^ textObjects;

				[Input("Rebuild Cache", IsSingle = true,IsBang=true, DefaultValue = 1, Order = 5)]
				ISpread<bool>^ rebuildCache;

				[Input("Enabled", IsSingle = true, DefaultValue = 1, Order = 10)]
				ISpread<bool>^ FInEnabled;

				[Output("Layer", IsSingle = true)]
				ISpread<DX11Resource<DX11Layer^>^>^ FOutLayer;

				void Render(DX11RenderContext^ ctx, DX11RenderSettings^ settings);

				Dictionary<DX11RenderContext^, IntPtr>^ fontrenderers;
				int spmax;
				SlimDX::DirectWrite::Factory^ dwFactory;

				DX11TextObjectCache^ textCache;
			};
		}
	}
}