using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Queries;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "PipelineStatistics", Category = "DX11.Query", Version = "", Author = "vux", Tags = "debug")]
    public class PipelineQueryNode : AbstractQueryNode<DX11PipelineQuery>
    {
        [Output("Compute Shader Invocations", IsSingle = true)]
        protected ISpread<int> FOutCSI;

        [Output("Input Assembler Primitives", IsSingle = true)]
        protected ISpread<int> FOutIAP;

        [Output("Input Assembler Vertices", IsSingle = true)]
        protected ISpread<int> FOutIAV;

        [Output("Vertex Shader Invocations", IsSingle = true)]
        protected ISpread<int> FOutVSI;

        [Output("Hull Shader Invocations", IsSingle = true)]
        protected ISpread<int> FOutHSI;

        [Output("Domain Shader Invocations", IsSingle = true)]
        protected ISpread<int> FOutDSI;

        [Output("Geometry Shader Invocations", IsSingle = true)]
        protected ISpread<int> FOutGSI;

        [Output("Geometry Shader Primitives", IsSingle = true)]
        protected ISpread<int> FOutGSP;

        [Output("Pixel Shader Invocations", IsSingle = true)]
        protected ISpread<int> FOutPSI;

        [Output("Rasterized Primitives", IsSingle = true)]
        protected ISpread<int> FOutRAP;

        [Output("Rendered Primitives", IsSingle = true)]
        protected ISpread<int> FOutREP;

        protected override DX11PipelineQuery CreateQueryObject(DX11RenderContext context)
        {
            return new DX11PipelineQuery(context);
        }

        protected override void OnEvaluate()
        {
            if (this.queryobject != null)
            {
                this.FOutCSI[0] = (int)this.queryobject.Statistics.ComputeShaderInvocations;
                this.FOutDSI[0] = (int)this.queryobject.Statistics.DomainShaderInvocations;
                this.FOutGSI[0] = (int)this.queryobject.Statistics.GeometryShaderInvocations;
                this.FOutGSP[0] = (int)this.queryobject.Statistics.GeometryShaderPrimitives;
                this.FOutHSI[0] = (int)this.queryobject.Statistics.HullShaderInvocations;
                this.FOutIAP[0] = (int)this.queryobject.Statistics.InputAssemblerPrimitives;
                this.FOutIAV[0] = (int)this.queryobject.Statistics.InputAssemblerVertices;
                this.FOutPSI[0] = (int)this.queryobject.Statistics.PixelShaderInvocations;
                this.FOutVSI[0] = (int)this.queryobject.Statistics.VertexShaderInvocations;
                this.FOutRAP[0] = (int)this.queryobject.Statistics.RasterizedPrimitives;
                this.FOutREP[0] = (int)this.queryobject.Statistics.RenderedPrimitives;
            }
        }
    }
}
