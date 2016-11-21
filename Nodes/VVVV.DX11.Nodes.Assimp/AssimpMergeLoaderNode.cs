using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using System.IO;
using FeralTic.Resources.Geometry;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D11;
using SlimDX;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name = "Geometry", Category = "DX11", Version = "Assimp.Merge", Author = "vux,flateric")]
    public class AssimpSimpleLoaderMergeNode : IPluginEvaluate, IDisposable, IDX11ResourceHost
    {
        [Input("Path", StringType = StringType.Filename, IsSingle=true)]
        protected IDiffSpread<string> FInPath;

        [Input("Reload", IsBang = true, IsSingle = true)]
        protected ISpread<bool> FInReload;

        [Output("Output", Order = 5, IsSingle=true)]
        protected ISpread<DX11Resource<DX11IndexOnlyGeometry>> FOutGeom;

        [Output("Position Buffer", Order = 5, IsSingle = true)]
        protected ISpread<DX11Resource<IDX11ReadableStructureBuffer>> FOutPosition;

        [Output("Id Buffer", Order = 6, IsSingle = true)]
        protected ISpread<DX11Resource<IDX11ReadableStructureBuffer>> FOutId;

        [Output("Uv Buffer", Order = 7)]
        protected ISpread<DX11Resource<IDX11ReadableStructureBuffer>> FOutUvs;

        [Output("Indices Buffer", Order = 8, IsSingle=true)]
        protected ISpread<DX11Resource<DX11RawBuffer>> FOutIndices;

        [Output("Vertices Count", Order = 9)]
        protected ISpread<int> FOutVerticesCount;

        [Output("Indices Count", Order = 10)]
        protected ISpread<int> FOutIndicesCount;

        [Output("Is Valid",Order=11)]
        protected ISpread<bool> FOutValid;

        private AssimpScene scene;

        private bool FInvalidate;
        private bool FEmpty = true;

        public AssimpSimpleLoaderMergeNode()
        {
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutGeom[0] == null)
            {
                this.FOutGeom[0] = new DX11Resource<DX11IndexOnlyGeometry>();
                this.FOutId[0] = new DX11Resource<IDX11ReadableStructureBuffer>();
                this.FOutIndices[0] = new DX11Resource<DX11RawBuffer>();
                this.FOutUvs[0] = new DX11Resource<IDX11ReadableStructureBuffer>();
            }

            this.FInvalidate = false;
            if (this.FInPath.IsChanged || this.FInReload[0])
            {
                this.DisposeResources();

                try
                {
                    AssimpScene scene = new AssimpScene(this.FInPath[0], true, false);
                    this.scene = scene;
                }
                catch
                {
                    this.scene = null;
                }
            }

            this.FInvalidate = true;

        }

        public void Dispose()
        {
            this.DisposeResources();
        }

        private void DisposeResources()
        {
            if (scene != null) { scene.Dispose(); }
            if (this.FOutGeom[0] != null) { this.FOutGeom[0].Dispose(); }
            if (this.FOutPosition[0] != null) { this.FOutPosition[0].Dispose(); }
            if (this.FOutUvs[0] != null) { this.FOutUvs[0].Dispose(); }
            this.scene = null;
        }

        private int currentid;
        private int vertexoffset = 0;
        private List<Vector3> vp = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<Matrix> nodetransform = new List<Matrix>();

        private List<int> indexbuffer = new List<int>();

        private void AppendNode(AssimpNode node)
        {
            foreach (AssimpNode child in node.Children)
            {
                this.AppendNode(child);
            }

            /*if (node.MeshCount > 0)
            {
                for (int i = 0; i < node.MeshCount;i++)
                {
                    this.AppendMesh(node, this.scene.Meshes[node.MeshIndices[i]);
                }
            }*/
        }

        private void AppendMesh(AssimpNode node, AssimpMesh mesh)
        {
            List<int> inds = mesh.Indices;

            if (inds.Count > 0 && mesh.VerticesCount > 0)
            {
                for (int idx = 0; idx < inds.Count; idx++)
                {
                    inds[idx] += vertexoffset;
                }

                indexbuffer.AddRange(inds);

                //vp.AddRange(mesh.v)

                vertexoffset += mesh.VerticesCount;
                this.currentid++;
            }
        }

        public void Update(DX11RenderContext context)
        {
            if ( this.scene == null) {return;}

            if (this.FInvalidate || !this.FOutGeom[0].Contains(context))
            {
                this.AppendNode(this.scene.RootNode);

                /*for (int i = 0; i < this.scenes.Count; i++)
                {
                    if (scenes[i] != null)
                    {
                        AssimpScene scene = scenes[i];

                        for (int j = 0; j < scene.MeshCount; j++)
                        {
                            AssimpMesh assimpmesh = scene.Meshes[j];

                            List<int> inds = assimpmesh.Indices;

                            if (inds.Count > 0 && assimpmesh.VerticesCount > 0)
                            {
                                var indexstream = new DataStream(inds.Count * 4, true, true);
                                indexstream.WriteRange(inds.ToArray());
                                indexstream.Position = 0;


                                DX11IndexOnlyGeometry geom = new DX11IndexOnlyGeometry(context);
                                geom.IndexBuffer = new DX11IndexBuffer(context, indexstream, false, true);
                                geom.InputLayout = assimpmesh.GetInputElements().ToArray();
                                geom.Topology = PrimitiveTopology.TriangleList;
                                geom.HasBoundingBox = true;
                                geom.BoundingBox = assimpmesh.BoundingBox;


                                DX11DynamicStructuredBuffer<Vector3> p =
                                    new DX11DynamicStructuredBuffer<Vector3>(context, assimpmesh.PositionPointer, assimpmesh.VerticesCount);

                                DX11DynamicStructuredBuffer<Vector3> n =
                                    new DX11DynamicStructuredBuffer<Vector3>(context, assimpmesh.NormalsPointer, assimpmesh.VerticesCount);

                                if (assimpmesh.UvChannelCount > 0)
                                {
                                    
                                    DX11DynamicStructuredBuffer<Vector3> u =
                                    new DX11DynamicStructuredBuffer<Vector3>(context, assimpmesh.GetUvPointer(0), assimpmesh.VerticesCount);


                                    this.FOutUvs[i][j][context] = u;
                                }

                                


                                DX11RawBuffer rb = new DX11RawBuffer(context, geom.IndexBuffer.Buffer);

                                this.FOutPosition[i][j][context] = p;
                                this.FOutNormals[i][j][context] = n;
                                this.FOutGeom[i][j][context] = geom;
                                this.FOutIndices[i][j][context] = rb;
                            }



                        }
                    }*/
               /* }
                this.FInvalidate = false;
                this.FEmpty = false;*/
            }    
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            
        }
    }
}
