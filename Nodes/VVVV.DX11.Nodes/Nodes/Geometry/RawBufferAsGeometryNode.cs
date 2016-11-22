using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FeralTic.DX11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using FeralTic.DX11.Resources;
using SlimDX.Direct3D11;
using SlimDX;

namespace VVVV.DX11.Nodes.Geometry
{
    [PluginInfo(Name = "AsGeometry", Category = "DX11.Buffer", Version = "Advanced", Author = "vux", Help = "Allows to set a raw buffer as drawing geometry by relocation memory addresses")]
    public class RawBufferAsGeometryNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In", IsSingle = true)]
        protected Pin<DX11Resource<IDX11Buffer>> inputBuffer;

        [Input("Allow Index Buffer")]
        protected ISpread<bool> allowIndexBuffer;

        [Input("Index Buffer Offset")]
        protected ISpread<int> indexBufferOffset;

        [Input("Allow Vertex Buffer")]
        protected ISpread<bool> allowVertexBuffer;

        [Input("Vertex Buffers Count", DefaultValue =1)]
        protected ISpread<int> vertexBufferCount;

        [Input("Vertex Buffer Offsets")]
        protected ISpread<int> vertexBufferOffsets;

        [Input("Vertex Buffer Strides")]
        protected ISpread<int> vertexBufferStrides;

        [Input("Draw Argument Offset")]
        protected ISpread<int> drawArgumentOffset;

        [Input("Topology", DefaultEnumEntry ="TriangleList")]
        protected ISpread<PrimitiveTopology> topology;

        [Input("Input Layout")]
        protected ISpread<InputElement> inputLayout;

        [Input("Enabled", DefaultValue = 1)]
        protected IDiffSpread<bool> enabled;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<RawBufferGeometry>> geometryOutput;

        private DX11Resource<RawBufferGeometry> rawGeometry;


        public void Evaluate(int SpreadMax)
        {
            if (this.geometryOutput[0] == null)
            {
                this.geometryOutput[0] = new DX11Resource<RawBufferGeometry>();
                this.rawGeometry = this.geometryOutput[0];
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (!this.rawGeometry.Contains(context))
            {
                this.rawGeometry[context] = new RawBufferGeometry(context);
            }

            if (this.inputBuffer.IsConnected && enabled[0])
            {
                var rg = this.rawGeometry[context];
                rg.Buffer = this.inputBuffer[0][context];
                rg.Topology = this.topology[0];
                rg.InputLayout = this.inputLayout.ToArray();
                rg.Prop.AllowIndexBuffer = this.allowIndexBuffer[0];
                rg.Prop.DrawOffset = this.drawArgumentOffset[0];
                rg.Prop.IndexBufferOffset = this.indexBufferOffset[0];

                rg.Prop.VertexBufferOffsets = new int[this.vertexBufferCount[0]];
                rg.Prop.VertexBufferStrides = new int[this.vertexBufferCount[0]];
                for (int i = 0; i < this.vertexBufferCount[0]; i++)
                {
                    rg.Prop.VertexBufferOffsets[i] = this.vertexBufferOffsets[i];
                    rg.Prop.VertexBufferStrides[i] = this.vertexBufferStrides[i];
                }
            }
            else
            {
                this.geometryOutput[0].Remove(context);
            }
        }
       
        public void Destroy(DX11RenderContext context, bool force)
        {

        }
    }
}
