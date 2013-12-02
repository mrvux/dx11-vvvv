using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using SlimDX;

using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using FeralTic.DX11.Geometry;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Mesh", Category = "DX11.Geometry Join Subsets", Version = "3d", Author = "velcrome")]
    public class MeshJoinNode : DX11AbstractMeshNode, IPluginEvaluate
    {
        #region Fields
        [Input("Vertices", AutoValidate=false)]
        protected ISpread<ISpread<Vector3>> FVertexPin;

        [Input("Normals", AutoValidate = false, DefaultValues = new double[]{0.0, 0.0,-1.0})]
        protected ISpread<ISpread<Vector3>> FNormalsPin;

        [Input("Texture Coords", AutoValidate=false)]
        protected ISpread<ISpread<Vector2>> FTexPin;

        [Input("Indices", AutoValidate = false, DefaultValues = new double[]{0.0, 1.0, 2.0})]
        protected ISpread<ISpread<Vector3>> FIndexPin;

        [Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = true)] 
        protected IDiffSpread<bool> FUpdate;
        #endregion

        #region constructor
        [ImportingConstructor()]
        public MeshJoinNode(IPluginHost Host) : base(Host) {}

        override protected void SetInputs() {
            // nothing to do here
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (FUpdate != null && FUpdate[0])
            {
                this.FVertex.Clear();
                this.FIndices.Clear();

                FVertexPin.Sync();
                FNormalsPin.Sync();
                FTexPin.Sync();
                
                FIndexPin.Sync();

                int meshCount = Math.Max(FVertexPin.SliceCount, FTexPin.SliceCount);
                meshCount = Math.Max(meshCount, FNormalsPin.SliceCount);
                meshCount = Math.Max(meshCount, FIndexPin.SliceCount);

                for (int i = 0; i < meshCount; i++)
                {

                    int vertexCount = Math.Max(FVertexPin[i].SliceCount, FTexPin[i].SliceCount);    

                    Pos4Norm3Tex2Vertex[] verts = new Pos4Norm3Tex2Vertex[Convert.ToInt32(vertexCount)];

                    for (int j = 0; j < vertexCount; j++)
                    {
                        verts[j].Position = new Vector4(FVertexPin[i][j], 1.0f);
                        verts[j].Normals = FNormalsPin[i][j];
                        verts[j].TexCoords =FTexPin[i][j];
                    }
                    this.FVertex.Add(verts);

                    List<int> inds = new List<int>();

                    for (int j = 0; j < FIndexPin[i].SliceCount; j++)
                    {
                        Vector3 triangle = FIndexPin[i][j];
                        inds.Add((int)triangle.X);
                        inds.Add((int)triangle.Y);
                        inds.Add((int)triangle.Z);
                    }
                    this.FIndices.Add(inds.ToArray());
                }
                this.InvalidateMesh(meshCount);
            }
        }
        #endregion

    }


}
