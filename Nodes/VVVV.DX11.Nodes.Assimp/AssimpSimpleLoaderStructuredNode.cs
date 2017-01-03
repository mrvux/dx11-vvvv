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
    [PluginInfo(Name = "Geometry", Category = "DX11", Version = "Assimp.Structured", Author = "vux,flateric")]
    public class AssimpSimpleLoaderStructuredNode : IPluginEvaluate, IDisposable, IDX11ResourceHost
    {
        [Input("Path", StringType = StringType.Filename)]
        protected IDiffSpread<string> FInPath;

        [Input("Reload", IsBang = true, IsSingle = true)]
        protected ISpread<bool> FInReload;

        [Output("Output", Order = 5)]
        protected ISpread<ISpread<DX11Resource<DX11IndexOnlyGeometry>>> FOutGeom;

        [Output("Position Buffer", Order = 5)]
        protected ISpread<ISpread<DX11Resource<IDX11ReadableStructureBuffer>>> FOutPosition;

        [Output("Normals Buffer", Order = 6)]
        protected ISpread<ISpread<DX11Resource<IDX11ReadableStructureBuffer>>> FOutNormals;

        [Output("Uv Buffer", Order = 7)]
        protected ISpread<ISpread<DX11Resource<IDX11ReadableStructureBuffer>>> FOutUvs;

        [Output("Indices Buffer", Order = 8)]
        protected ISpread<ISpread<DX11Resource<DX11RawBuffer>>> FOutIndices;

        [Output("Vertices Count", Order = 9)]
        protected ISpread<ISpread<int>> FOutVerticesCount;

        [Output("Indices Count", Order = 10)]
        protected ISpread<ISpread<int>> FOutIndicesCount;

        [Output("Is Valid",Order=11)]
        protected ISpread<bool> FOutValid;

        private List<AssimpScene> scenes = new List<AssimpScene>();

        private bool FInvalidate;
        private bool FEmpty = true;

        public AssimpSimpleLoaderStructuredNode()
        {
        }

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;
            if (this.FInPath.IsChanged || this.FInReload[0])
            {
                this.DisposeResources();

                this.FOutGeom.SliceCount = this.FInPath.SliceCount;
                this.FOutIndices.SliceCount = this.FInPath.SliceCount;
                this.FOutIndicesCount.SliceCount = this.FInPath.SliceCount;
                this.FOutNormals.SliceCount = this.FInPath.SliceCount;
                this.FOutPosition.SliceCount = this.FInPath.SliceCount;
                this.FOutVerticesCount.SliceCount = this.FInPath.SliceCount;
                this.FOutUvs.SliceCount = this.FInPath.SliceCount;

                for (int i = 0; i < this.FInPath.SliceCount; i++)
                {
                    try
                    {
                        AssimpScene scene = new AssimpScene(this.FInPath[i],true, false);
                        this.FOutGeom[i].SliceCount = scene.MeshCount;
                        this.FOutPosition[i].SliceCount = scene.MeshCount;
                        this.FOutNormals[i].SliceCount = scene.MeshCount;
                        this.FOutIndicesCount[i].SliceCount = scene.MeshCount;
                        this.FOutVerticesCount[i].SliceCount = scene.MeshCount;
                        this.FOutIndices[i].SliceCount = scene.MeshCount;
                        this.FOutUvs[i].SliceCount = scene.MeshCount;

                        for (int j = 0; j < this.FOutGeom[i].SliceCount;j++ )
                        {
                            this.FOutGeom[i][j] = new DX11Resource<DX11IndexOnlyGeometry>();
                            this.FOutPosition[i][j] = new DX11Resource<IDX11ReadableStructureBuffer>();
                            this.FOutNormals[i][j] = new DX11Resource<IDX11ReadableStructureBuffer>();
                            this.FOutIndicesCount[i][j] = scene.Meshes[j].Indices.Count;
                            this.FOutVerticesCount[i][j] = scene.Meshes[j].VerticesCount;
                            this.FOutIndices[i][j] = new DX11Resource<DX11RawBuffer>();
                            this.FOutUvs[i][j] = new DX11Resource<IDX11ReadableStructureBuffer>();
                        }
                        this.scenes.Add(scene);
                    }
                    catch
                    {
                        this.scenes.Add(null);
                        this.FOutGeom[i].SliceCount = 0;
                    }
                }

                this.FInvalidate = true;
            }
        }

        public void Dispose()
        {
            this.DisposeResources();
        }

        private void DisposeResources()
        {
            foreach (AssimpScene scene in this.scenes)
            {
                if (scene != null) { scene.Dispose(); }
            }

            for (int i = 0; i < this.FOutGeom.SliceCount; i++)
            {
                for (int j = 0; j < this.FOutGeom[i].SliceCount; j++)
                {
                    if (this.FOutGeom[i][j] != null) { this.FOutGeom[i][j].Dispose(); }
                    if (this.FOutPosition[i][j] != null) { this.FOutPosition[i][j].Dispose(); }
                    if (this.FOutNormals[i][j] != null) { this.FOutNormals[i][j].Dispose(); }
                }
            }

            this.scenes.Clear();
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInvalidate || this.FEmpty)
            {
                for (int i = 0; i < this.scenes.Count; i++)
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
                    }
                }
                this.FInvalidate = false;
                this.FEmpty = false;
            }     
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            
        }
    }
}
