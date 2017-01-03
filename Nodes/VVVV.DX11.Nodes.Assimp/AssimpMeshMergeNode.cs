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
using FeralTic.DX11.Geometry;
using System.Runtime.InteropServices;

namespace VVVV.DX11.Nodes.AssetImport
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Pos3Norm3Tex2InstanceVertex
    {
        public Vector3 Position;
        public Vector3 Normals;
        public Vector2 TexCoords;
        public Vector3 Center;
        public int InstanceID;

        private static InputElement[] layout;

        public static InputElement[] Layout
        {
            get
            {
                if (layout == null)
                {
                    layout = new InputElement[]
                    {
                        new InputElement("POSITION",0,SlimDX.DXGI.Format.R32G32B32_Float, InputElement.AppendAligned, 0),
                        new InputElement("NORMAL",0,SlimDX.DXGI.Format.R32G32B32_Float,InputElement.AppendAligned,0),
                        new InputElement("TEXCOORD",0,SlimDX.DXGI.Format.R32G32_Float,InputElement.AppendAligned,0),
                         new InputElement("CENTER",0,SlimDX.DXGI.Format.R32G32B32_Float,InputElement.AppendAligned,0),
                        new InputElement("INSTANCEID",0,SlimDX.DXGI.Format.R32_UInt,InputElement.AppendAligned,0),
                    };
                }
                return layout;
            }
        }

        public static int VertexSize
        {
            get { return Marshal.SizeOf(typeof(Pos3Norm3Tex2InstanceVertex)); }
        }
    }

    [PluginInfo(Name="MergeMesh",Category="DX11.Geometry",Version="Assimp",Author="vux,flateric")]
    public unsafe class AssimpMeshMergeNode : IPluginEvaluate,IDisposable,IDX11ResourceHost
    {
        [Input("Scene",IsSingle=true)]
        protected IDiffSpread<AssimpScene> FInScene;

        [Input("Node Name")]
        protected ISpread<string> FInNodeName;

        [Input("Use Name")]
        protected IDiffSpread<bool> FinUseName;

        [Input("Reload", IsBang = true)]
        protected ISpread<bool> FReload;


        [Output("Output", Order = 5, IsSingle=true)]
        protected Pin<DX11Resource<DX11IndexedGeometry>> FOutGeom;

        [Output("UV Buffer", Order = 5, IsSingle = true)]
        protected Pin<DX11Resource<DX11ImmutableStructuredBuffer>> FOutUVBuffer;

        [Output("Buffer", Order = 5, IsSingle = true)]
        protected Pin<DX11Resource<DX11ImmutableStructuredBuffer>> FOutBuffer;

        [Output("Indices", Order = 7, IsSingle = true)]
        protected Pin<DX11Resource<DX11RawBuffer>> FOutIndices;

        [Output("Transforms", Order = 11)]
        protected ISpread<Matrix> FOutTransforms;

        [Output("UV Channel Count", Order = 12)]
        protected ISpread<int> FOutUVChannelCount;

        private bool FInvalidate = false;

        private List<int> idsort = new List<int>();

        [ImportingConstructor()]
        public AssimpMeshMergeNode(IPluginHost host)
        {
        }

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;
            if (this.FInScene.IsChanged || this.FReload[0])
            {
                //Destroy old mesh
                if (this.FOutGeom[0] != null) 
                { 
                    this.FOutGeom[0].Dispose(); 
                    this.FOutBuffer[0].Dispose();
                    this.FOutUVBuffer[0].Dispose();
                }

                if (this.FInScene[0] != null)
                {
                    if (this.FOutGeom[0] == null) 
                    { 
                        this.FOutGeom[0] = new DX11Resource<DX11IndexedGeometry>(); 
                        this.FOutBuffer[0] = new DX11Resource<DX11ImmutableStructuredBuffer>(); 
                        this.FOutIndices[0] = new DX11Resource<DX11RawBuffer>();
                        this.FOutUVBuffer[0] = new DX11Resource<DX11ImmutableStructuredBuffer>();
                    }

                    if (this.FinUseName[0])
                    {
                       

                        idsort.Clear();
                        this.FOutGeom.SliceCount = 1;
                        this.FOutTransforms.SliceCount = this.FInNodeName.SliceCount;
                        

                        for (int i = 0; i < this.FInNodeName.SliceCount; i++)
                        {
                            found = null;
                            RecurseNodesByName(this.FInScene[0].RootNode, this.FInNodeName[i]);

                            if (found == null)
                            {
                                this.FOutTransforms[i] = Matrix.Identity;
                            }
                            else
                            {
                                this.FOutTransforms[i] = found.RelativeTransform;
                            }
                            idsort.Add(found.MeshIndices[0]);
                        }

                        
                    }
                    else
                    {
                        int meshcnt = this.FInScene[0].MeshCount;

                        this.FOutGeom.SliceCount = 1;
                        this.FOutTransforms.SliceCount = meshcnt;

                        for (int i = 0; i < meshcnt; i++)
                        {
                            found = null;
                            RecurseNodes(this.FInScene[0].RootNode, i);

                            if (found == null)
                            {
                                this.FOutTransforms[i] = Matrix.Identity;
                            }
                            else
                            {
                                this.FOutTransforms[i] = found.RelativeTransform;
                            }
                        }

                    }

                    this.FOutUVChannelCount[0] = this.FInScene[0].Meshes[0].UvChannelCount;
                }
                else
                {
                    this.FOutTransforms.SliceCount = 0;
                    this.FOutGeom.SliceCount = 0;
                }
                this.FInvalidate = true;
            }
        }

        private AssimpNode found;

        private void RecurseNodes(AssimpNode node,int meshidx)
        {
            for (int i = 0;i < node.MeshCount; i++)
            {
                if (meshidx == node.MeshIndices[i])
                {
                    found = node;
                    return;
                }
            }

            for (int i = 0; i < node.Children.Count;i++)
            {
                RecurseNodes(node.Children[i], meshidx);
            }
        }

        private void RecurseNodesByName(AssimpNode node, string name)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                if (name == node.Name)
                {
                    found = node;
                    return;
                }
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                RecurseNodesByName(node.Children[i], name);
            }
        }

        private int GetUVChannelCount(AssimpMesh assimpmesh, int slot)
        {
            var texcd = assimpmesh.GetInputElements().Where(ie => ie.SemanticName == "TEXCOORD" && ie.SemanticIndex == slot).FirstOrDefault();
            int cnt = 2;
            if (texcd != null)
            {
                if (texcd.Format == SlimDX.DXGI.Format.R32G32B32_Float)
                {
                    cnt = 3;
                }
            }
            return cnt;
        }

        public void Update(DX11RenderContext context)
        {

            if (this.FInvalidate || !this.FOutGeom[0].Contains(context))
            {
                int vertexoffset = 0;

                List<Pos3Norm3Tex2InstanceVertex> vertices = new List<Pos3Norm3Tex2InstanceVertex>();
                List<int> indices = new List<int>();
                List<Vector2> uvs = new List<Vector2>();

                int cnt = this.FinUseName[0] ? idsort.Count : this.FInScene[0].MeshCount;

                for (int i = 0; i < cnt; i++)
                {
                    AssimpMesh assimpmesh = this.FinUseName[0] == false ? this.FInScene[0].Meshes[i] : this.FInScene[0].Meshes[idsort[i]];

                    List<int> inds = assimpmesh.Indices;

                    

                    if (inds.Count > 0 && assimpmesh.VerticesCount > 0)
                    {
                        var texcd = assimpmesh.GetInputElements().Where( ie => ie.SemanticName == "TEXCOORD").FirstOrDefault();
                        bool zuv = false;
                        if (texcd != null)
                        {
                            zuv = texcd.Format == SlimDX.DXGI.Format.R32G32B32_Float;
                        }
                        for (int j = 0; j < inds.Count; j++)
                        {
                            indices.Add(inds[j] + vertexoffset);
                        }

                        DataStream posbuffer = new DataStream(assimpmesh.PositionPointer, assimpmesh.VerticesCount * 12, true, true);
                        DataStream normbuffer = new DataStream(assimpmesh.NormalsPointer, assimpmesh.VerticesCount * 12, true, true);
                        DataStream uvbuffer = null;

                        List<DataStream> uvbuffers = new List<DataStream>();
                        List<int> uvcounters = new List<int>();

                        for (int uvc = 0; uvc < assimpmesh.UvChannelCount;uvc++ )
                        {
                            uvbuffers.Add(new DataStream(assimpmesh.GetUvPointer(uvc), assimpmesh.VerticesCount * 12, true, true));
                            uvcounters.Add(this.GetUVChannelCount(assimpmesh, uvc));
                        }

                        if (assimpmesh.UvChannelCount > 0)
                        {
                            uvbuffer = new DataStream(assimpmesh.GetUvPointer(0), assimpmesh.VerticesCount * 12, true, true);
                        }

                        Vector3* pos = (Vector3*)posbuffer.DataPointer.ToPointer();
                        Vector3 accum = Vector3.Zero;
                        for (int j = 0; j < assimpmesh.VerticesCount; j++)
                        {
                            accum += pos[j];
                        }
                        Vector3 center = accum / assimpmesh.VerticesCount;

                        for (int j = 0; j < assimpmesh.VerticesCount; j++)
                        {
                            Pos3Norm3Tex2InstanceVertex vert = new Pos3Norm3Tex2InstanceVertex()
                            {
                                InstanceID = i,
                                Normals = normbuffer.Read<Vector3>(),
                                Position = posbuffer.Read<Vector3>(),
                                Center = center,
                                TexCoords = uvbuffer != null ? uvbuffer.Read<Vector2>() : Vector2.Zero
                            };
                            vertices.Add(vert);

                            for (int k = 0; k < assimpmesh.UvChannelCount; k++ )
                            {
                                var b = uvbuffers[k];
                                uvs.Add(b.Read<Vector2>());

                                if (uvcounters[k] == 3)
                                {
                                    b.Read<float>();
                                }

                            }

                            if (uvbuffer != null && zuv) { uvbuffer.Read<float>(); }
                        }
                        vertexoffset += assimpmesh.VerticesCount;
                    }
                }

                DataStream vS = new DataStream(vertices.ToArray(), true, true);
                vS.Position = 0;

                DataStream iS = new DataStream(indices.ToArray(), true, true);
                iS.Position = 0;

                var vbuffer = new SlimDX.Direct3D11.Buffer(context.Device, vS, new BufferDescription()
                {
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    SizeInBytes = (int)vS.Length,
                    Usage = ResourceUsage.Default
                });


                DX11IndexedGeometry geom = new DX11IndexedGeometry(context);
                geom.VertexBuffer = vbuffer;
                geom.IndexBuffer = new DX11IndexBuffer(context, iS, true, false);
                geom.InputLayout = Pos3Norm3Tex2InstanceVertex.Layout;
                geom.Topology = PrimitiveTopology.TriangleList;
                geom.VerticesCount = vertices.Count;
                geom.VertexSize = Pos3Norm3Tex2InstanceVertex.VertexSize;
                geom.HasBoundingBox = false;

                vS.Position = 0;

                DataStream uvS = new DataStream(uvs.ToArray(), true, true);
                uvS.Position = 0;

                this.FOutGeom[0][context] = geom;
                this.FOutBuffer[0][context] = new DX11ImmutableStructuredBuffer(context.Device, geom.VerticesCount, geom.VertexSize, vS);
                this.FOutIndices[0][context] = new DX11RawBuffer(context, geom.IndexBuffer.Buffer);
                this.FOutUVBuffer[0][context] = new DX11ImmutableStructuredBuffer(context.Device,uvs.Count,8,uvS);

                this.FInvalidate = false;

                
            }          
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (this.FOutGeom[0] != null) { this.FOutGeom[0].Dispose(context); }
            if (this.FOutBuffer[0] != null) { this.FOutBuffer[0].Dispose(context); }
        }

        #region IDisposable Members
        public void Dispose()
        {
            if (this.FOutGeom[0] != null) { this.FOutGeom[0].Dispose(); }
            if (this.FOutBuffer[0] != null) { this.FOutBuffer[0].Dispose(); }
        }
        #endregion



    }
}
