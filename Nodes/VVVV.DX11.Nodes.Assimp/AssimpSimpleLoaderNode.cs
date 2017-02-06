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
    [PluginInfo(Name = "GeometryFile", Category = "DX11.Geometry", Version = "Assimp", Author = "vux,flateric")]
    public class AssimpSimpleLoaderNode : IPluginEvaluate, IDisposable, IDX11ResourceHost
    {
        [Input("Filename", StringType = StringType.Filename)]
        protected IDiffSpread<string> FInPath;

        [Input("Reload", IsBang = true, IsSingle = true)]
        protected ISpread<bool> FInReload;

        [Input("Keep In Memory", IsSingle = true)]
        protected ISpread<bool> FInKeep;

        [Output("Output", Order = 1)]
        protected ISpread<ISpread<DX11Resource<DX11IndexedGeometry>>> FOutGeom;

        //[Output("Mesh Count", IsSingle = true)]
        //ISpread<int> FOutMeshCount;

        [Output("Is Valid", Order=5)]
        protected ISpread<bool> FOutValid;

        private List<AssimpScene> scenes = new List<AssimpScene>();

        private bool FInvalidate;
        private bool FEmpty = true;

        public AssimpSimpleLoaderNode()
        {
        }

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;
            if (this.FInPath.IsChanged || this.FInReload[0])
            {
                this.DisposeResources();

                this.FOutGeom.SliceCount = this.FInPath.SliceCount;
                this.FOutValid.SliceCount = this.FInPath.SliceCount;

                for (int i = 0; i < this.FInPath.SliceCount; i++)
                {
                    try
                    {
                        AssimpScene scene = new AssimpScene(this.FInPath[i],true, false);
                        this.FOutGeom[i].SliceCount = scene.MeshCount;
                        for (int j = 0; j < this.FOutGeom[i].SliceCount;j++ )
                        {
                            this.FOutGeom[i][j] = new DX11Resource<DX11IndexedGeometry>();
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


                            DataStream vS = assimpmesh.Vertices;
                            vS.Position = 0;

                            List<int> inds = assimpmesh.Indices;

                            if (inds.Count > 0 && assimpmesh.VerticesCount > 0)
                            {

                                var vertices = new SlimDX.Direct3D11.Buffer(context.Device, vS, new BufferDescription()
                                {
                                    BindFlags = BindFlags.VertexBuffer,
                                    CpuAccessFlags = CpuAccessFlags.None,
                                    OptionFlags = ResourceOptionFlags.None,
                                    SizeInBytes = (int)vS.Length,
                                    Usage = ResourceUsage.Default
                                });

                                var indexstream = new DataStream(inds.Count * 4, true, true);
                                indexstream.WriteRange(inds.ToArray());
                                indexstream.Position = 0;


                                DX11IndexedGeometry geom = new DX11IndexedGeometry(context);
                                geom.VertexBuffer = vertices;
                                geom.IndexBuffer = new DX11IndexBuffer(context, indexstream, false, true);
                                geom.InputLayout = assimpmesh.GetInputElements().ToArray();
                                geom.Topology = PrimitiveTopology.TriangleList;
                                geom.VerticesCount = assimpmesh.VerticesCount;
                                geom.VertexSize = assimpmesh.CalculateVertexSize();
                                geom.HasBoundingBox = true;
                                geom.BoundingBox = assimpmesh.BoundingBox;

                                this.FOutGeom[i][j][context] = geom;
                            }



                        }
                    }
                }
                this.FEmpty = false;
                this.FInvalidate = false;
            }     
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (force || !this.FInKeep[0])
            {
                for (int i = 0; i < this.FOutGeom.SliceCount; i++)
                {
                    for (int j = 0; j < this.FOutGeom[i].SliceCount; j++)
                    {
                        this.FOutGeom[i][j].Dispose(context);
                    }
                }
                this.FEmpty = true;
            }
        }
    }
}
