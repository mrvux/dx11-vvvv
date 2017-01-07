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
using System.Runtime.InteropServices;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name="Mesh",Category="Geometry Split",Version="Assimp",Author="vux,flateric")]
    public unsafe class AssimpMeshSplitNode : IPluginEvaluate
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

        [Input("Scene",IsSingle=true)]
        protected IDiffSpread<AssimpScene> scene;

        [Input("Mesh Index", IsSingle = true)]
        protected IDiffSpread<int> meshindex;

        [Output("Position", Order = 5, BinName="Vertices Count", BinOrder=4)]
        protected ISpread<ISpread<Vector3>> position;

        [Output("Normals", Order = 6, BinVisibility=PinVisibility.OnlyInspector)]
        protected ISpread<ISpread<Vector3>> normals;

        [Output("UV Channel 1", Order = 7, BinVisibility = PinVisibility.OnlyInspector)]
        protected ISpread<ISpread<float>> uv1;

        [Output("UV Channel Count", Order = 10, BinVisibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> uvchancount;

        [Output("Indices", Order = 15, BinName="Indices Count", BinOrder=16)]
        protected ISpread<ISpread<int>> indices;


        [ImportingConstructor()]
        public AssimpMeshSplitNode(IPluginHost host)
        {
        }

        public void Evaluate(int SpreadMax)
        { 
            if (this.scene.IsChanged || this.meshindex.IsChanged)
            {
                if (this.scene[0] != null)
                {
                    int meshcnt = this.scene[0].MeshCount;

                    if (this.meshindex[0] < 0)
                    {
                        this.position.SliceCount = meshcnt;
                        this.normals.SliceCount = meshcnt;
                        this.uv1.SliceCount = meshcnt;
                        this.uvchancount.SliceCount = meshcnt;
                        this.indices.SliceCount = meshcnt;

                        for (int i = 0; i < meshcnt; i++)
                        {
                            this.WriteMesh(this.scene[0].Meshes[i], i);
                        }
                    }
                    else
                    {
                        int meshidx = this.meshindex[0] % this.scene[0].MeshCount;

                        this.position.SliceCount = 1;
                        this.normals.SliceCount = 1;
                        this.uv1.SliceCount = 1;
                        this.uvchancount.SliceCount = 1;
                        this.indices.SliceCount = 1;

                        this.WriteMesh(this.scene[0].Meshes[meshidx], 0);
                    }
                }
                else
                {
                    this.position.SliceCount = 0;
                    this.normals.SliceCount = 0;
                    this.uv1.SliceCount = 0;
                    this.uvchancount.SliceCount = 0;
                    this.indices.SliceCount = 0;
                }

                this.position.Flush(true);
                this.normals.Flush(true);
                this.uv1.Flush(true);
                this.indices.Flush(true);
            }
        }

        private void WriteMesh(AssimpMesh mesh, int slice)
        {
            this.position[slice].SliceCount = mesh.VerticesCount;

            fixed (Vector3* vptr = &this.position[slice].Stream.Buffer[0])
            {
                memcpy(new IntPtr(vptr), mesh.PositionPointer, mesh.VerticesCount * 12);
            }

            if (mesh.HasNormals)
            {
                this.normals[slice].SliceCount = mesh.VerticesCount;
                fixed (Vector3* nptr = &this.normals[slice].Stream.Buffer[0])
                {
                    memcpy(new IntPtr(nptr), mesh.NormalsPointer, mesh.VerticesCount * 12);
                }
            }
            else
            {
                this.normals[slice].SliceCount = 0;
            }

            if (mesh.UvChannelCount > 0)
            {
                int chancnt = this.GetChannelCount(mesh, 0);
                this.uv1[slice].SliceCount = mesh.VerticesCount * chancnt;
                this.uvchancount[slice] = chancnt;

                fixed (float* uptr = &this.uv1[slice].Stream.Buffer[0])
                {
                    memcpy(new IntPtr(uptr), mesh.GetUvPointer(0), mesh.VerticesCount * 4 * chancnt);
                }
            }
            else
            {
                this.uv1[slice].SliceCount = 0;
                this.uvchancount[slice] = 0;
            }

            this.indices[slice].SliceCount = mesh.Indices.Count;
            var inds = mesh.Indices;
            var ibo = this.indices[slice].Stream.Buffer;
            for (int i = 0; i < mesh.Indices.Count; i++)
            {
                ibo[i] = inds[i];
            }
        }

        private int GetChannelCount(AssimpMesh mesh, int slot)
        {
            var fmt = mesh.GetInputElements().Where(ie => ie.SemanticName == "TEXCOORD" && ie.SemanticIndex == slot).First();

            switch(fmt.Format)
            {
                case SlimDX.DXGI.Format.R32_Float:
                    return 1;
                case SlimDX.DXGI.Format.R32G32_Float:
                    return 2;
                case SlimDX.DXGI.Format.R32G32B32_Float:
                    return 3;
                case SlimDX.DXGI.Format.R32G32B32A32_Float:
                    return 4;
                default :
                    return 0;
            }

        }
    }
}
