using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using VVVV.DX11.Lib.Rendering;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "RenderSemantic", Category = "DX11", Version = "RWStructuredBuffer")]
    public class DX11RWBufferSemanticNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Input")]
        protected Pin<DX11Resource<IDX11RWStructureBuffer>> FInput;

        [Input("Semantic", DefaultString = "RWSEMANTIC")]
        protected ISpread<string> FSemantic;

        [Input("Mandatory", DefaultValue = 0)]
        protected ISpread<bool> FMandatory;

        [Output("Output")]
        protected ISpread<DX11Resource<StructuredBufferRenderSemantic>> FOutput;

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<StructuredBufferRenderSemantic>(); }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInput.IsConnected)
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    this.FOutput[i][context] = new StructuredBufferRenderSemantic(this.FSemantic[i], this.FMandatory[i]);

                    if (this.FInput[i].Contains(context))
                    {
                        this.FOutput[i][context].Data = this.FInput[i][context];
                    }
                    else
                    {
                        this.FOutput[i][context].Data = null;
                    }
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                this.FOutput[i].Dispose(context);
            }
        }
    }
}
