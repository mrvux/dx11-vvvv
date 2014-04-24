using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D11;

using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "DepthStencil", Category = "DX11.RenderState",Version="Advanced", Author = "vux,tonfilm")]
    public class DX11DepthStencilStateNode : IPluginEvaluate
    {
        [Input("Render State",CheckIfChanged=true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Comparison", DefaultEnumEntry = "Always")]
        protected IDiffSpread<Comparison> FInComparison;

        [Input("Depth Write Mask", DefaultEnumEntry = "Zero")]
        protected IDiffSpread<DepthWriteMask> FInDepthWriteMask;

        [Input("Enable Depth")]
        protected IDiffSpread<bool> FInEnableDepth;

        [Input("Enable Stencil")]
        protected IDiffSpread<bool> FInEnableStencil;

        [Input("Stencil Read Mask",DefaultValue=255,MaxValue=255,MinValue=0)]
        protected IDiffSpread<int> FInStencilReadMask;

        [Input("Stencil Write Mask", DefaultValue = 255, MaxValue = 255, MinValue = 0)]
        protected IDiffSpread<int> FInStencilWriteMask;

        [Input("BackFace Comparison")]
        protected IDiffSpread<Comparison> FInBFComp;

        [Input("BackFace Depth Fail Op")]
        protected IDiffSpread<StencilOperation> FInBFDFOp;

        [Input("BackFace Fail Op")]
        protected IDiffSpread<StencilOperation> FInBFFOp;

        [Input("BackFace Pass Op")]
        protected IDiffSpread<StencilOperation> FInBFPOp;

        [Input("FrontFace Comparison")]
        protected IDiffSpread<Comparison> FInFFComp;

        [Input("FrontFace Depth Fail Op")]
        protected IDiffSpread<StencilOperation> FInFFDFOp;

        [Input("FrontFace Fail Op")]
        protected IDiffSpread<StencilOperation> FInFFFOp;

        [Input("FrontFace Pass Op")]
        protected IDiffSpread<StencilOperation> FInFFPOp;

        [Output("Render State")]
        protected ISpread<DX11RenderState> FOutState;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInComparison.IsChanged
                || this.FInDepthWriteMask.IsChanged
                || this.FInEnableDepth.IsChanged
                || this.FInEnableStencil.IsChanged
                || this.FInState.IsChanged
                || this.FInBFComp.IsChanged
                || this.FInBFDFOp.IsChanged
                || this.FInBFFOp.IsChanged
                || this.FInBFPOp.IsChanged
                || this.FInFFComp.IsChanged
                || this.FInFFDFOp.IsChanged
                || this.FInFFFOp.IsChanged
                || this.FInFFPOp.IsChanged
                || this.FInStencilReadMask.IsChanged
                || this.FInStencilWriteMask.IsChanged)
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

                    DepthStencilStateDescription ds = rs.DepthStencil;
                    ds.DepthComparison = this.FInComparison[i];
                    ds.DepthWriteMask = this.FInDepthWriteMask[i];
                    ds.IsDepthEnabled = this.FInEnableDepth[i];
                    ds.IsStencilEnabled = this.FInEnableStencil[i];
                    int srm = Math.Min(255, Math.Max(this.FInStencilReadMask[i], 0));
                    int swm = Math.Min(255, Math.Max(this.FInStencilWriteMask[i], 0));
                    ds.StencilReadMask = Convert.ToByte(srm);
                    ds.StencilWriteMask = Convert.ToByte(swm);

                    DepthStencilOperationDescription dbf = new DepthStencilOperationDescription();
                    dbf.Comparison = this.FInBFComp[i];
                    dbf.DepthFailOperation = this.FInBFDFOp[i];
                    dbf.FailOperation = this.FInBFFOp[i];
                    dbf.PassOperation = this.FInBFPOp[i];

                    ds.BackFace = dbf;


                    DepthStencilOperationDescription dff = new DepthStencilOperationDescription();
                    dff.Comparison = this.FInFFComp[i];
                    dff.DepthFailOperation = this.FInFFDFOp[i];
                    dff.FailOperation = this.FInFFFOp[i];
                    dff.PassOperation = this.FInFFPOp[i];
  
                    ds.FrontFace = dff;
                    rs.DepthStencil = ds;
                    this.FOutState[i] = rs;
                }

            }
            
        }
    }
}
