using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using SlimDX.DXGI;
using VVVV.DX11.Internals;
using FeralTic.DX11.Utils;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name="IndexedGeometryBuffer",Category="DX11.Geometry",Version="Join",Author="vux,tonfilm")]
    public class IndexedGeometryBufferJoinNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        IPluginHost FHost;

        [Input("Vertices", AutoValidate=false)]
        protected ISpread<float> FInput;

        [Input("Vertices Count", AutoValidate = false,DefaultValue=1)]
        protected ISpread<int> FInVerticesCount;

        [Input("Indices", AutoValidate = false)]
        protected ISpread<int> FInIndices;

        [Input("Indices Count", AutoValidate = false,DefaultValue=3)]
        protected ISpread<int> FIndicesCount;

        [Input("Input Layout", CheckIfChanged = true, AutoValidate = false)]
        protected Pin<InputElement> FInLayout;

        [Input("Topology", Order = 2, AutoValidate = false)]
        protected ISpread<PrimitiveTopology> FInTopology;

        [Input("Apply",IsBang=true,DefaultValue=1)]
        protected ISpread<bool> FInApply;

        [Output("Geometry Out",IsSingle=true)]
        protected Pin<DX11Resource<DX11IndexedGeometry>> FOutput;

        bool FInvalidate;
        DataStream FVertexStream;
        DataStream FIndexStream;
        private int vertexsize;
        private InputElement[] inputlayout;
        private bool FFirst = true;

        [ImportingConstructor()]
        public IndexedGeometryBufferJoinNode(IPluginHost host)
        {
            this.FHost = host;     
        }

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;
            if (this.FOutput[0] == null)
            {
                this.FOutput[0] = new DX11Resource<DX11IndexedGeometry>();
            }

            if (this.FInApply[0] || FFirst)
            {
                this.FFirst = false;
                this.FInput.Sync();
                this.FInVerticesCount.Sync();
                this.FInIndices.Sync();
                this.FIndicesCount.Sync();
                this.FInLayout.Sync();
                this.FInTopology.Sync();

                this.FInvalidate = true;
                DX11Resource<DX11IndexedGeometry> instance = this.FOutput[0];
                this.FOutput[0] = instance;

                //Get Input Layout
                this.inputlayout = new InputElement[this.FInLayout.SliceCount];
                this.vertexsize = 0;
                for (int i = 0; i < this.FInLayout.SliceCount; i++)
                {

                    if (this.FInLayout.IsConnected && this.FInLayout[i] != null)
                    {
                        this.inputlayout[i] = this.FInLayout[i];
                    }
                    else
                    {
                        //Set deault, can do better here
                        this.inputlayout[i] = new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0);
                    }
                    this.vertexsize += FormatHelper.Instance.GetSize(this.inputlayout[i].Format);
                }
                InputLayoutFactory.AutoIndex(this.inputlayout);
             
                //Load Vertex Stream
                if (this.FVertexStream != null) { this.FVertexStream.Dispose(); }

                this.FVertexStream = new DataStream(this.FInVerticesCount[0] * this.vertexsize, true, true);

                for (int i = 0; i < this.FInVerticesCount[0] * (this.vertexsize / 4); i++)
                {
                    this.FVertexStream.Write(this.FInput[i]);
                }
                this.FVertexStream.Position = 0;

                //Load index stream
                if (this.FIndexStream != null) { this.FIndexStream.Dispose(); }

                this.FIndexStream = new DataStream(this.FIndicesCount[0] * 4, true, true);
                for (int i = 0; i < this.FIndicesCount[0]; i++)
                {
                    this.FIndexStream.Write(this.FInIndices[i]);
                }
                this.FIndexStream.Position = 0;

            }
        }

        #region IDX11Resource Members
        public void Update(DX11RenderContext context)
        {
            if (this.FInvalidate)
            {
                if (this.FOutput[0].Contains(context)) { this.FOutput[0].Dispose(context); }

                DX11IndexedGeometry geom = new DX11IndexedGeometry(context);
                geom.InputLayout = this.inputlayout;
                geom.VertexSize = this.vertexsize;
  
                var vertices = new SlimDX.Direct3D11.Buffer(context.Device, this.FVertexStream, new BufferDescription()
                {
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    SizeInBytes = (int)this.FVertexStream.Length,
                    Usage = ResourceUsage.Default
                });

                geom.HasBoundingBox = false;

                geom.IndexBuffer = new DX11IndexBuffer(context, this.FIndexStream, false, false);

                geom.InputLayout = this.inputlayout;
                geom.Topology = this.FInTopology[0];

                geom.VertexBuffer = vertices;
                geom.VertexSize = this.vertexsize;
                geom.VerticesCount = this.FInVerticesCount[0];

                this.FOutput[0][context] = geom;

                this.FInvalidate = false;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutput[0].Dispose(context);
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            if (this.FOutput[0] != null) { this.FOutput[0].Dispose(); }
        }
        #endregion
    }
}
