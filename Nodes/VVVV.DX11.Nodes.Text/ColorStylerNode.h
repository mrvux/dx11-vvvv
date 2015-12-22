#pragma once

#include "FW1FontWrapper.h"
#include <map>

#include "FontWrapperFactory.h"

//using namespace VVVV::PluginInterfaces::V2;
using namespace VVVV::PluginInterfaces::V1;

using namespace FeralTic::DX11;
using namespace FeralTic::DX11::Resources;
using namespace FeralTic::Utils;

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
				virtual void Apply(SlimDX::DirectWrite::TextLayout^ layout) 
				{ 
					if (Enabled)
					{
						IFW1Factory* factory = FontWrapperFactory::GetFactory();

						IFW1ColorRGBA *pColor;
						factory->CreateColor(0x00000000, &pColor);

						pColor->SetColor(this->Color.Red, this->Color.Green, this->Color.Blue, this->Color.Alpha);

						IDWriteTextLayout* lay = (IDWriteTextLayout*)layout->ComPointer.ToPointer();

						DWRITE_TEXT_RANGE rng;
						rng.startPosition = this->Range->StartPosition;
						rng.length = this->Range->Length;
						lay->SetDrawingEffect(pColor, rng);
						pColor->Release();
					}
				}

				System::Boolean Enabled;
				SlimDX::Color4 Color;
				SlimDX::DirectWrite::TextRange^ Range;
			};

			[PluginInfo(Name = "Color", Author = "vux", Category = "DirectWrite", Version="Styles")]
			public ref class ColorStylerNode : public IPluginEvaluate
			{
			public:
				[ImportingConstructor()]
				ColorStylerNode(IIOFactory^ iofactory)
				{
					InputAttribute^ colorAttribute = gcnew InputAttribute("Color");
					colorAttribute->Order = 6;
					colorAttribute->DefaultColor = MagicNumberUtils::WhiteDefault();

					this->FInColor = IOFactoryExtensions::CreateSpread<SlimDX::Color4>(iofactory, colorAttribute, true);
				}

				virtual void Evaluate(int spreadMax) 
				{
					for (int i = 0; i < spreadMax; i++)
					{
						ColorStyler^  ts = gcnew ColorStyler();
						SlimDX::DirectWrite::TextRange tr;
						tr.StartPosition = VVVV::DX11::Lib::Utils::SpreadUtils::GetItem(from, i);
						tr.Length = VVVV::DX11::Lib::Utils::SpreadUtils::GetItem(length, i);
						ts->Range = tr;
						ts->Enabled = VVVV::DX11::Lib::Utils::SpreadUtils::GetItem(enabled, i);
						ts->Color = this->FInColor[i];
						this->styleOut[i] = ts;
					}
				}
			private:
				VVVV::PluginInterfaces::V2::ISpread<SlimDX::Color4>^ FInColor;

				[Input("From", Order = 500)]
				VVVV::PluginInterfaces::V2::IDiffSpread<int>^ from;

				[Input("Length", Order = 501, DefaultValue = 1)]
				VVVV::PluginInterfaces::V2::IDiffSpread<int>^ length;

				[Input("Enabled", Order = 502, DefaultValue = 1)]
				VVVV::PluginInterfaces::V2::IDiffSpread<bool>^ enabled;

				[Output("Style Out")]
				VVVV::PluginInterfaces::V2::ISpread<ITextStyler^>^ styleOut;
			};
		}
	}
}
