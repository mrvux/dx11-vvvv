using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Composition;


using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;


namespace VVVV.DX11.Nodes
{
    public enum e2dMeshTextureMapping { Stretch, Crop }

    public abstract class DX11AbstractMeshNode : IDX11ResourceHost
    {
        #region Fields and abstract stuff
        protected IPluginHost FHost;

        protected List<Pos4Norm3Tex2Vertex[]> FVertex = new List<Pos4Norm3Tex2Vertex[]>();
        protected List<int[]> FIndices = new List<int[]>();
        protected bool FInvalidate;

        protected abstract void SetInputs();

        [Output("Geometry Out", Order = 5)]
        protected Pin<DX11Resource<DX11IndexedGeometry>> FOutput;
        #endregion

        [ImportingConstructor()]
        public DX11AbstractMeshNode(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            this.SetInputs();
        }

        #region Dispose
        public void Dispose()
        {
            this.FOutput.SafeDisposeAll();
        }
        #endregion




        protected void InvalidateMesh(int slicecount)
        {
            this.FInvalidate = true;

            this.FOutput.SliceCount = slicecount;
            for (int i = 0; i < slicecount; i++)
            {
                if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<DX11IndexedGeometry>(); }
            }
        }


        public void Update(DX11RenderContext context)
        {

            if (this.FInvalidate || !this.FOutput[0].Contains(context))
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    if (this.FOutput[i].Contains(context))
                    {
                        this.FOutput[i].Dispose(context);
                    }
                }

                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    Pos4Norm3Tex2Vertex[] vertices = this.FVertex[i];
                    int[] indices = this.FIndices[i];

                    DataStream ds = new DataStream(vertices.Length * Pos4Norm3Tex2Vertex.VertexSize, true, true);
                    ds.Position = 0;

                    ds.WriteRange(vertices);

                    ds.Position = 0;

                    var vbuffer = new SlimDX.Direct3D11.Buffer(context.Device, ds, new BufferDescription()
                    {
                        BindFlags = BindFlags.VertexBuffer,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None,
                        SizeInBytes = (int)ds.Length,
                        Usage = ResourceUsage.Default
                    });

                    ds.Dispose();

                    var indexstream = new DataStream(indices.Length * 4, true, true);
                    indexstream.WriteRange(indices);
                    indexstream.Position = 0;

                    DX11IndexedGeometry geom = new DX11IndexedGeometry(context);
                    geom.VertexBuffer = vbuffer;
                    geom.IndexBuffer = new DX11IndexBuffer(context, indexstream,false, true);
                    geom.InputLayout = Pos4Norm3Tex2Vertex.Layout;
                    geom.Topology = PrimitiveTopology.TriangleList;
                    geom.VerticesCount = vertices.Length ;
                    geom.VertexSize = Pos4Norm3Tex2Vertex.VertexSize;

                    geom.HasBoundingBox = false;

                    this.FOutput[i][context] = geom;
                }
            }
            this.FInvalidate = false;
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutput.SafeDisposeAll(context);
        }
    }
}
