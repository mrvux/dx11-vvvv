using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
	[VVVV.PluginInterfaces.V2.PluginInfo(Name = "Rasterizer", Category = "DX11.RenderState", Version="Advanced", Tags="fill, point, wireframe, solid", Author = "vux,tonfilm")]
	public class DX11RasterizerStateNode : IPluginEvaluate
	{
		[Input("Render State",CheckIfChanged=true)]
        protected Pin<DX11RenderState> FInState;

		[Input("Fill Mode",DefaultEnumEntry="Solid")]
        protected IDiffSpread<FillMode> FInFillMode;

		[Input("Cull Mode",DefaultEnumEntry="None")]
        protected IDiffSpread<CullMode> FInCullMode;

		[Input("Depth Bias")]
        protected IDiffSpread<int> FInDepthBias;

		[Input("Depth Bias Clamp")]
        protected IDiffSpread<float> FInDepthBiasClamp;

		[Input("Enable Depth Clip")]
        protected IDiffSpread<bool> FInDepthClipEnable;

		[Input("Enable Line AntiAlias")]
        protected IDiffSpread<bool> FInLineAAEnable;

		[Input("Enable MultiSampling")]
        protected IDiffSpread<bool> FInEnableMS;

		[Input("Enable Scissor")]
        protected IDiffSpread<bool> FInEnableScissor;

		[Input("Slope Scaled Depth Bias")]
        protected IDiffSpread<float> FInSlopeScaleDB;

		[Input("Is Front Cull CCW")]
        protected IDiffSpread<bool> FInFrontCCW;

		[Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

		public void Evaluate(int SpreadMax)
		{
			if (this.FInFillMode.IsChanged 
				|| this.FInCullMode.IsChanged
				|| this.FInDepthBias.IsChanged
				|| this.FInDepthBiasClamp.IsChanged
				|| this.FInDepthClipEnable.IsChanged
				|| this.FInFrontCCW.IsChanged
				|| this.FInLineAAEnable.IsChanged
				|| this.FInEnableMS.IsChanged
				|| this.FInEnableScissor.IsChanged
				|| this.FInSlopeScaleDB.IsChanged
				|| this.FInState.IsChanged)
			{
				this.FOutState.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax; i++)
				{
					DX11RenderState rs;
					if (this.FInState.PluginIO.IsConnected)
					{
                        rs = this.FInState[i].Clone();
					}
					else
					{
						rs = new DX11RenderState();
					}

					RasterizerStateDescription rsd = rs.Rasterizer;
					rsd.FillMode = this.FInFillMode[i];
					rsd.CullMode = this.FInCullMode[i];
					rsd.DepthBias = this.FInDepthBias[i];
					rsd.DepthBiasClamp = this.FInDepthBiasClamp[i];
					rsd.IsDepthClipEnabled = this.FInDepthClipEnable[i];
					rsd.IsAntialiasedLineEnabled = this.FInLineAAEnable[i];
					rsd.IsFrontCounterclockwise = this.FInFrontCCW[i];
					rsd.IsMultisampleEnabled = this.FInEnableMS[i];
					rsd.IsScissorEnabled = this.FInEnableScissor[i];
					rsd.SlopeScaledDepthBias = this.FInSlopeScaleDB[i];

					rs.Rasterizer = rsd;
                    
					this.FOutState[i] = rs;

				}
			}
		}
	}
}
