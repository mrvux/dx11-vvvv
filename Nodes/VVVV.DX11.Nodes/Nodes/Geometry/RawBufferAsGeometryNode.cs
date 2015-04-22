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
    public class RawBufferAsGeometryNode : IPluginEvaluate, IDX11ResourceProvider
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

        private RawBufferGeometry rawGeometry;


        public void Evaluate(int SpreadMax)
        {
            if (this.geometryOutput[0] == null)
            {
                this.geometryOutput[0] = new DX11Resource<RawBufferGeometry>();
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (this.rawGeometry == null)
            {
                this.rawGeometry = new RawBufferGeometry(context);
            }

            if (this.inputBuffer.IsConnected && enabled[0])
            {
                this.rawGeometry.Buffer = this.inputBuffer[0][context];
                this.rawGeometry.Topology = this.topology[0];
                this.rawGeometry.InputLayout = this.inputLayout.ToArray();
                this.rawGeometry.Prop.AllowIndexBuffer = this.allowIndexBuffer[0];
                this.rawGeometry.Prop.DrawOffset = this.drawArgumentOffset[0];
                this.rawGeometry.Prop.IndexBufferOffset = this.indexBufferOffset[0];

                this.rawGeometry.Prop.VertexBufferOffsets = new int[this.vertexBufferCount[0]];
                this.rawGeometry.Prop.VertexBufferStrides = new int[this.vertexBufferCount[0]];
                for (int i = 0; i < this.vertexBufferCount[0]; i++)
                {
                    this.rawGeometry.Prop.VertexBufferOffsets[i] = this.vertexBufferOffsets[i];
                    this.rawGeometry.Prop.VertexBufferStrides[i] = this.vertexBufferStrides[i];
                }

                this.geometryOutput[0][context] = this.rawGeometry;
            }
            else
            {
                this.geometryOutput[0].Data.Remove(context);
            }
        }
       
        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {

        }
    }
}
