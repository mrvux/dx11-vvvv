#include "stdafx.h"
#include "TextSettings.h"
#include "DX11TextLayerAdvNode.h"

#include "FontWrapperFactory.h"


using namespace FeralTic::Utils;

namespace VVVV {
	namespace Nodes {
		namespace DX11 {

			DX11TextSettingsNode::DX11TextSettingsNode(SlimDX::DirectWrite::Factory^ dwFactory)
			{
				this->dwFactory = dwFactory;
			}

			void DX11TextSettingsNode::Evaluate(int SpreadMax)
			{
				if (this->FOutTextWrapper[0] == nullptr)
				{
					this->FOutTextWrapper[0] = gcnew DX11Resource<TextFontRenderer^>();
				}

				if (this->sheetSizeX->IsChanged || this->sheetSizeY->IsChanged
					|| this->glyphWidth->IsChanged || this->glyphHeight->IsChanged
					|| this->sheetMips->IsChanged || this->aniso->IsChanged
					)
				{
					VVVV::DX11::DX11ResourceExtensions::SafeDisposeAll(this->FOutTextWrapper);
				}

			}

			void DX11TextSettingsNode::Update(DX11RenderContext^ context)
			{
				if (!this->FOutTextWrapper[0]->Contains(context))
				{
					FW1_FONTWRAPPERCREATEPARAMS createParams = { 0 };
					createParams.GlyphSheetWidth =sheetSizeX->Stream[0];
					createParams.GlyphSheetHeight = sheetSizeY->Stream[0];
					createParams.MaxGlyphWidth = glyphWidth->Stream[0];
					createParams.MaxGlyphHeight = glyphHeight->Stream[0];
					createParams.SheetMipLevels = sheetMips->Stream[0];
					createParams.AnisotropicFiltering = aniso->Stream[0];
					createParams.DefaultFontParams.pszFontFamily = L"Arial";
					createParams.DefaultFontParams.FontWeight = DWRITE_FONT_WEIGHT_NORMAL;
					createParams.DefaultFontParams.FontStyle = DWRITE_FONT_STYLE_NORMAL;
					createParams.DefaultFontParams.FontStretch = DWRITE_FONT_STRETCH_NORMAL;

					ID3D11Device* dev = (ID3D11Device*)context->Device->ComPointer.ToPointer();
					IDWriteFactory* dw = (IDWriteFactory*)dwFactory->ComPointer.ToPointer();

					this->FOutTextWrapper[0][context] = gcnew TextFontRenderer(dev, dw, &createParams);
				}
			}


			void DX11TextSettingsNode::Destroy(DX11RenderContext^ context, bool force)
			{
				if (force)
				{ 
					if (this->FOutTextWrapper[0] != nullptr)
					{
						this->FOutTextWrapper[0]->Dispose(context);
					}
				}
			}

			DX11TextSettingsNode::~DX11TextSettingsNode()
			{
				if (this->FOutTextWrapper[0] != nullptr)
				{
					delete this->FOutTextWrapper[0];
				}
			}
		}
	}
}