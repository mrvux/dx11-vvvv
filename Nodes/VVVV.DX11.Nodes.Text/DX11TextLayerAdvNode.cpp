#include "StdAfx.h"
#include "DX11TextLayerAdvNode.h"

namespace VVVV {
	namespace Nodes {
		namespace DX11 {

			DX11TextLayerAdvNode::DX11TextLayerAdvNode(IIOFactory^ factory, SlimDX::DirectWrite::Factory^ dwFactory)
			{
				this->dwFactory = dwFactory;
				this->iofactory = factory;
				factory->PluginHost->CreateTransformInput("Transform In", TSliceMode::Dynamic, TPinVisibility::True, this->FInTr);
				this->FInTr->Order = 1;
				this->fontrenderers = gcnew	Dictionary<DX11RenderContext^, IntPtr>();
			}

			void DX11TextLayerAdvNode::Evaluate(int SpreadMax)
			{
				this->spmax = SpreadMax;

				if (this->FOutLayer[0] == nullptr)
				{
					this->FOutLayer[0] = gcnew DX11Resource<DX11Layer^>();
				}
			}

			void DX11TextLayerAdvNode::Update(IPluginIO^ pin, DX11RenderContext^ context)
			{
				if (!this->FOutLayer[0]->Contains(context))
				{
					this->FOutLayer[0][context] = gcnew DX11Layer();
					this->FOutLayer[0][context]->Render = gcnew RenderDelegate<DX11RenderSettings^>(this, &DX11TextLayerAdvNode::Render);
				}

				if (!this->fontrenderers->ContainsKey(context))
				{
					FW1_FONTWRAPPERCREATEPARAMS createParams = { 0 };
					createParams.SheetMipLevels = 5;
					createParams.AnisotropicFiltering = TRUE;
					createParams.DefaultFontParams.pszFontFamily = L"Arial";
					createParams.DefaultFontParams.FontWeight = DWRITE_FONT_WEIGHT_NORMAL;
					createParams.DefaultFontParams.FontStyle = DWRITE_FONT_STYLE_NORMAL;
					createParams.DefaultFontParams.FontStretch = DWRITE_FONT_STRETCH_NORMAL;


					IFW1Factory *pFW1Factory;
					FW1CreateFactory(FW1_VERSION, &pFW1Factory);
					ID3D11Device* dev = (ID3D11Device*)context->Device->ComPointer.ToPointer();

					IFW1FontWrapper* pw;

					IDWriteFactory* dw = (IDWriteFactory*)this->dwFactory->ComPointer.ToPointer();

					pFW1Factory->CreateFontWrapper(dev, dw, &createParams, &pw);
					pFW1Factory->Release();

					this->fontrenderers->Add(context, IntPtr(pw));
				}
			}


			void DX11TextLayerAdvNode::Destroy(IPluginIO^ pin, DX11RenderContext^ context, bool force)
			{
				if (this->FOutLayer[0]->Contains(context))
				{
					this->FOutLayer[0]->Dispose(context);
				}
			}

			void DX11TextLayerAdvNode::Render(IPluginIO^ pin, DX11RenderContext^ context, DX11RenderSettings^ settings)
			{
				if (this->FInEnabled[0] && this->FLayout->PluginIO->IsConnected)
				{
					float w = settings->RenderWidth;
					float h = settings->RenderHeight;

					IFW1FontWrapper* fw = (IFW1FontWrapper*)this->fontrenderers[context].ToPointer();

					ID3D11Device* dev = (ID3D11Device*)context->Device->ComPointer.ToPointer();
					ID3D11DeviceContext* pContext = (ID3D11DeviceContext*)context->CurrentDeviceContext->ComPointer.ToPointer();

					int cnt;
					float* tr;
					this->FInTr->GetMatrixPointer(cnt, tr);

					SlimDX::Matrix* smp = (SlimDX::Matrix*)&tr[0];

					for (int i = 0; i < this->spmax; i++)
					{

						SlimDX::Color4 c = this->FInColor[i];
						c.Red = this->FInColor[i].Blue;
						c.Blue = this->FInColor[i].Red;
						unsigned int color = c.ToArgb();


						SlimDX::Matrix preScale = SlimDX::Matrix::Scaling(1.0f, -1.0f, 1.0f);
						/*switch (this->FNormalizeInput[i]->Index)
						{
						case 1: preScale = SlimDX::Matrix::Scaling(1.0f / w, -1.0f / w, 1.0f); break;
						case 2: preScale = SlimDX::Matrix::Scaling(1.0f / h, -1.0f / h, 1.0f); break;
						case 3: preScale = SlimDX::Matrix::Scaling(1.0f / w, -1.0f / h, 1.0f); break;
						}*/

						SlimDX::Matrix sm = smp[i % this->FInTr->SliceCount];

						SlimDX::Matrix mat = SlimDX::Matrix::Multiply(preScale, sm);
						mat = SlimDX::Matrix::Multiply(mat, settings->View);
						mat = SlimDX::Matrix::Multiply(mat, settings->Projection);

						float* tr = (float*)&mat;

						FW1_RECTF rect = { 0.0f, 0.0f, 0.0f, 0.0f };
						int flag = 0;

						TextLayout^ tf = this->FLayout->Stream->Buffer[i % this->FLayout->Stream->Buffer->Length];
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