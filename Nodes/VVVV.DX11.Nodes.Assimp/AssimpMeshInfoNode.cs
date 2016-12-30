using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using SlimDX;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name = "Info", Category = "Assimp", Version = "Mesh", Author = "vux,flateric")]
    public class AssimpMeshInfoNode : IPluginEvaluate, IDisposable
    {
        [Input("Mesh",CheckIfChanged=true)]
        protected Pin<AssimpMesh> FInMeshes;

        [Output("Vertices Count")]
        protected ISpread<int> FOutVCount;

        [Output("Indices Count")]
        protected ISpread<int> FOutIndicesCount;

        [Output("Bounding Min")]
        protected ISpread<Vector3> FOutBoundingMin;

        [Output("Bounding Max")]
        protected ISpread<Vector3> FOutBoundingMax;

        [Output("Material Index", Order = 10)]
        protected ISpread<int> FOutMaterialIndex;

        [Output("Max Bones Per Vertex", Order = 10)]
        protected ISpread<int> FOutMaxBones;


        public void Evaluate(int SpreadMax)
        {
            if (this.FInMeshes.PluginIO.IsConnected)
            {
                if (this.FInMeshes.IsChanged)
                {
                    int meshcnt = this.FInMeshes.SliceCount;

                    this.FOutIndicesCount.SliceCount = meshcnt;
                    this.FOutVCount.SliceCount = meshcnt;
                    this.FOutMaterialIndex.SliceCount = meshcnt;
                    this.FOutBoundingMin.SliceCount = meshcnt;
                    this.FOutBoundingMax.SliceCount = meshcnt;
                    this.FOutMaxBones.SliceCount = meshcnt;

                    for (int i = 0; i < this.FInMeshes.SliceCount; i++)
                    {
                        AssimpMesh assimpmesh = this.FInMeshes[i];

                        this.FOutMaterialIndex[i] = assimpmesh.MaterialIndex;
                        this.FOutBoundingMin[i] = assimpmesh.BoundingBox.Minimum;
                        this.FOutBoundingMax[i] = assimpmesh.BoundingBox.Maximum;
                        this.FOutVCount[i] = assimpmesh.VerticesCount;
                        this.FOutIndicesCount[i] = assimpmesh.Indices.Count;
                        this.FOutMaxBones[i] = assimpmesh.MaxBonePerVertex;
                    }
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}
