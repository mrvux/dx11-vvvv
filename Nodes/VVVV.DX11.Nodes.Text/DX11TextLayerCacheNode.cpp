#include "StdAfx.h"
#include "DX11TextLayerCacheNode.h"

#include "FontWrapperFactory.h"

using namespace FeralTic::Utils;

namespace VVVV {
	namespace Nodes {
		namespace DX11 {

			DX11TextLayerCacheNode::DX11TextLayerCacheNode(SlimDX::DirectWrite::Factory^ dwFactory)
			{
				this->dwFactory = dwFactory;
				this->fontrenderers = gcnew	Dictionary<DX11RenderContext^, IntPtr>();
			}

			void DX11TextLayerCacheNode::Evaluate(int SpreadMax)
			{
				this->spmax = SpreadMax;

				if (this->FOutLayer[0] == nullptr)
				{
					this->FOutLayer[0] = gcnew DX11Resource<DX11Layer^>();
				}

				if (this->rebuildCache[0])
				{
					this->textObjects->Sync();
					this->textFormat->Sync();

					if (this->textCache != nullptr)
					{
						delete this->textCache;
						this->textCache = nullptr;
					}

					auto defaultTextFormat = this->textFormat[0];

					List<DX11CachedText^>^ cacheList = gcnew List<DX11CachedText^>();

					for (int i = 0; i < this->textObjects->SliceCount; i++)
					{
						TextObject^ to = this->textObjects[i];
						auto tFormat = to->TextFormat != nullptr ? to->TextFormat : defaultTextFormat;

						TextLayout^ tl = gcnew TextLayout(this->dwFactory, to->Text, tFormat);

						SlimDX::Color4 c = to->Color;
						c.Red = to->Color.Blue;
						c.Blue = to->Color.Red;

						DX11CachedText^ ct = gcnew DX11CachedText(tl, to->Matrix, c);
						cacheList->Add(ct);
					}

					this->textCache = gcnew DX11TextObjectCache(cacheList);
				}
			}

			void DX11TextLayerCacheNode::Update(DX11RenderContext^ context)
			{
				if (!this->FOutLayer[0]->Contains(context))
				{
					this->FOutLayer[0][context] = gcnew DX11Layer();
					this->FOutLayer[0][context]->Render = gcnew RenderDelegate<DX11RenderSettings^>(this, &DX11TextLayerCacheNode::Render);
				}

				if (!this->FTextRenderer->IsConnected)
				{
					if (!this->fontrenderers->ContainsKey(context))
					{
						IFW1FontWrapper* pw = FontWrapperFactory::GetWrapper(context, this->dwFactory);
						this->fontrenderers->Add(context, IntPtr(pw));
					}
				}
			}


			void DX11TextLayerCacheNode::Destroy(DX11RenderContext^ context, bool force)
			{
				if (this->FOutLayer[0]->Contains(context))
				{
					this->FOutLayer[0]->Dispose(context);
				}
			}

			void DX11TextLayerCacheNode::Render(DX11RenderContext^ context, DX11RenderSettings^ settings)
			{
				if (this->FInEnabled[0] && this->textCache != nullptr)
				{
					float w = (float)settings->RenderWidth;
					float h = (float)settings->RenderHeight;

					IFW1FontWrapper* fw = 0;
					if (this->FTextRenderer->IsConnected)
					{
						fw = (IFW1FontWrapper*)FTextRenderer[0][context]->NativePointer().ToPointer();
					}
					else
					{
						fw = (IFW1FontWrapper*)this->fontrenderers[context].ToPointer();
					}

					IFW1GlyphRenderStates* pRenderStates;
					fw->GetRenderStates(&pRenderStates);

					ID3D11Device* dev = (ID3D11Device*)context->Device->ComPointer.ToPointer();
					ID3D11DeviceContext* pContext = (ID3D11DeviceContext*)context->CurrentDeviceContext->ComPointer.ToPointer();

					for (int i = 0; i < this->textCache->objects->Length; i++)
					{
						auto tc = this->textCache->Get(i);
						unsigned int color = tc->Color.ToArgb();

						SlimDX::Matrix preScale = SlimDX::Matrix::Scaling(1.0f, -1.0f, 1.0f);
						SlimDX::Matrix sm = tc->Matrix;

						SlimDX::Matrix mat = SlimDX::Matrix::Multiply(preScale, sm);
						mat = SlimDX::Matrix::Multiply(mat, settings->View);
						mat = SlimDX::Matrix::Multiply(mat, settings->Projection);

						float* tr = (float*)&mat;

						FW1_RECTF rect = { 0.0f, 0.0f, 0.0f, 0.0f };
						int flag = 0;

						TextLayout^ tf = tc->TextLayout;
						fw->DrawTextLayout(pContext, (IDWriteTextLayout*)tf->ComPointer.ToPointer(), 0, 0, color, NULL, tr, 0);
					}

					//Apply old states back
					context->RenderStateStack->Apply();
					context->CleanShaderStages();
				}
			}

		}
	}
}