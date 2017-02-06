using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using AssimpNet;
using System.ComponentModel.Composition;
using SlimDX;
using SlimDX.Direct3D11;

using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name="Mesh",Category="DX11.Geometry",Version="Assimp",Author="vux,flateric")]
    public class AssimpMeshNode : IPluginEvaluate,IDisposable,IDX11ResourceHost
    {
        [Input("Scene",IsSingle=true)]
        protected IDiffSpread<AssimpScene> FInScene;

        [Output("Output")]
        protected Pin<DX11Resource<DX11IndexedGeometry>> FOutGeom;

        [Output("Bone Names")]
        protected ISpread<ISpread<string>> FOutBoneNames;

        [Output("Bone Matrices")]
        protected ISpread<ISpread<Matrix>> FOutBoneMats;

        [Output("Bounding Min")]
        protected ISpread<Vector3> FOutBoundingMin;

        [Output("Bounding Max")]
        protected ISpread<Vector3> FOutBoundingMax;

        [Output("Material Index",Order = 10)]
        protected ISpread<int> FOutMaterialIndex;

        [Output("Is Valid", Order = 11)]
        protected ISpread<bool> FOutValid;

        private bool FInvalidate = false;


        [ImportingConstructor()]
        public AssimpMeshNode(IPluginHost host)
        {
        }

        public void Evaluate(int SpreadMax)
        {
            
            this.FInvalidate = false;
            if (this.FInScene.IsChanged)
            {
                //Destroy old mesh
                for (int i = 0; i < this.FOutGeom.SliceCount; i++)
                {
                    if (this.FOutGeom[i] != null) { this.FOutGeom[i].Dispose(); }
                }

                if (this.FInScene[0] != null)
                {
                    int meshcnt = this.FInScene[0].MeshCount;

                    this.FOutGeom.SliceCount = meshcnt;
                    this.FOutMaterialIndex.SliceCount = meshcnt;
                    this.FOutBoneNames.SliceCount = meshcnt;
                    this.FOutBoneMats.SliceCount = meshcnt;
                    this.FOutValid.SliceCount = meshcnt;
                    this.FOutBoundingMin.SliceCount = meshcnt;
                    this.FOutBoundingMax.SliceCount = meshcnt;

                    for (int i = 0; i < meshcnt; i++)
                    {
                        AssimpMesh assimpmesh = this.FInScene[0].Meshes[i];

                        this.FOutMaterialIndex[i] = assimpmesh.MaterialIndex;

                        this.FOutBoneNames[i].AssignFrom(assimpmesh.BoneNames);
                        this.FOutBoneMats[i].SliceCount = assimpmesh.BoneMatrices.Count;
                        for (int j = 0; j < assimpmesh.BoneMatrices.Count; j++ )
                        {
                            this.FOutBoneNames[i][j] = assimpmesh.BoneNames[j];
                            this.FOutBoneMats[i][j] = Matrix.Transpose(assimpmesh.BoneMatrices[j]);
                        }
                        this.FOutGeom[i] = new DX11Resource<DX11IndexedGeometry>();
                        this.FOutValid[i] = assimpmesh.Indices.Count > 0 && assimpmesh.VerticesCount > 0;
                        this.FOutBoundingMin[i] = assimpmesh.BoundingBox.Minimum;
                        this.FOutBoundingMax[i] = assimpmesh.BoundingBox.Maximum;
                    }
                }
                else
                {
                    this.FOutGeom.SliceCount = 0;
                    this.FOutMaterialIndex.SliceCount = 0;
                    this.FOutBoneNames.SliceCount = 0;
                    this.FOutBoneMats.SliceCount = 0;
                    this.FOutValid.SliceCount = 0;
                    this.FOutBoundingMin.SliceCount = 0;
                    this.FOutBoundingMax.SliceCount = 0;
                }
                this.FInvalidate = true;
            }
        }

        public void Update(DX11RenderContext context)
        {

            if (this.FInvalidate || !this.FOutGeom[0].Contains(context))
            {
                for (int i = 0; i < this.FInScene[0].MeshCount; i++)
                {
                    AssimpMesh assimpmesh = this.FInScene[0].Meshes[i];

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


                        this.FOutGeom[i][context] = geom;
                    }
                    else
                    {
                        this.FOutGeom[i][context] = null;
                    }
                }
                this.FInvalidate = false;
            }          
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutGeom.SafeDisposeAll(context);
        }

        #region IDisposable Members
        public void Dispose()
        {
            this.FOutGeom.SafeDisposeAll();
        }
        #endregion



    }
}
