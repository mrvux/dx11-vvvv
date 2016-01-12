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

namespace VVVV { namespace Nodes { namespace DX11  {
	
	[StartableAttribute(Name = "Font Factory", Lazy=false)]
	public ref class FontWrapperFactory : public IStartable
	{
	public:
		virtual void Start()
		{
			fontfactory = 0;
			fontWrapper = 0;
		}

		virtual void Shutdown()
		{
			if (fontfactory != NULL)
			{
				fontfactory->Release();
			}
			if (fontWrapper != NULL)
			{
				fontWrapper->Release();
			}
		}

		static IFW1Factory* GetFactory()
		{
			if (fontfactory == nullptr)
			{
				IFW1Factory *pFW1Factory;
				FW1CreateFactory(FW1_VERSION, &pFW1Factory);

				fontfactory = pFW1Factory;
			}
			return fontfactory;
		}


		static IFW1FontWrapper* GetWrapper(DX11RenderContext^ context, SlimDX::DirectWrite::Factory^ dwFactory)
		{
			if (fontWrapper == nullptr)
			{
				IFW1Factory *pFW1Factory = GetFactory();

				FW1_FONTWRAPPERCREATEPARAMS createParams = { 0 };
				createParams.SheetMipLevels = 5;
				createParams.AnisotropicFiltering = TRUE;
				createParams.DefaultFontParams.pszFontFamily = L"Arial";
				createParams.DefaultFontParams.FontWeight = DWRITE_FONT_WEIGHT_NORMAL;
				createParams.DefaultFontParams.FontStyle = DWRITE_FONT_STYLE_NORMAL;
				createParams.DefaultFontParams.FontStretch = DWRITE_FONT_STRETCH_NORMAL;

				ID3D11Device* dev = (ID3D11Device*)context->Device->ComPointer.ToPointer();
				IDWriteFactory* dw = (IDWriteFactory*)dwFactory->ComPointer.ToPointer();
				IFW1FontWrapper* pw;

				pFW1Factory->CreateFontWrapper(dev, dw, &createParams, &pw);

				fontWrapper = pw;
			}
			return fontWrapper;
		}
	private:
		static IFW1Factory* fontfactory;
		static IFW1FontWrapper* fontWrapper;
	};
} } }

