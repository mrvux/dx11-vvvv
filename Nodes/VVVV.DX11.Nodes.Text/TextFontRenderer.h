#pragma once

#include "FW1FontWrapper.h"
#include <map>

using namespace FeralTic::DX11;
using namespace FeralTic::DX11::Resources;

using namespace VVVV::DX11;

using namespace System;

namespace VVVV {
	namespace Nodes {
		namespace DX11 {

			public ref class TextFontRenderer : public IDX11Resource
			{
			public:
			TextFontRenderer(ID3D11Device* d3ddevice, IDWriteFactory* dwFactory, FW1_FONTWRAPPERCREATEPARAMS* createParams);
			~TextFontRenderer();

			System::IntPtr NativePointer() { return this->mFontWrapperInstance; }

			private:
			System::IntPtr mFontWrapperInstance;
			};
		}
	}
}
