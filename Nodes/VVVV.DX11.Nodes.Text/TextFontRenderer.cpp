#include "stdafx.h"
#include "TextFontRenderer.h"

#include "FontWrapperFactory.h"


using namespace FeralTic::Utils;

namespace VVVV {
	namespace Nodes {
		namespace DX11 {

			TextFontRenderer::TextFontRenderer(ID3D11Device* d3ddevice, IDWriteFactory* dwFactory, FW1_FONTWRAPPERCREATEPARAMS* createParams)
			{
				IFW1Factory* factory = FontWrapperFactory::GetFactory();
				IFW1FontWrapper* pw;
				factory->CreateFontWrapper(d3ddevice, dwFactory, createParams, &pw);

				this->mFontWrapperInstance = System::IntPtr(pw);
			}

			TextFontRenderer::~TextFontRenderer()
			{
				IFW1FontWrapper* pw = (IFW1FontWrapper*)this->mFontWrapperInstance.ToPointer();
				pw->Release();
				pw = 0;
			}
		}
	}
}