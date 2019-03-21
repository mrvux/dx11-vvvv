using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;
using BulletSharp.SoftBody;

using VVVV.Internals.Bullet;
using FeralTic.DX11.Utils;
using VVVV.Bullet.Core;

namespace VVVV.DX11.Nodes.Bullet
{
	[PluginInfo(Name = "SoftBody", Category = "Bullet",Version="DX11.Geometry",
		Help = "Gets a soft body data as mesh", Author = "vux")]
	public class BulletGetSoftBodyMesh : IPluginEvaluate,IDX11ResourceHost, System.IDisposable
	{
		[Input("Bodies")]
        protected ISpread<SoftBody> FBodies;

		IPluginHost FHost;

        [Output("Output", Order = 5)]
        protected ISpread<DX11Resource<DX11IndexedGeometry>> FOutput;

        [Output("Is Valid", Order = 100)]
        protected ISpread<bool> FValid;

        [ImportingConstructor()]
		public BulletGetSoftBodyMesh(IPluginHost host)
		{
            this.FHost = host;
		}

		public void Evaluate(int SpreadMax)
		{
			
			this.FValid.SliceCount = SpreadMax;

			int validcnt = 0;
			for (int i = 0; i < SpreadMax; i++)
			{
				FValid[i] = this.FBodies[i].Faces.Count > 0 || this.FBodies[i].Tetras.Count > 0;
				if (FValid[i]) { validcnt++; }
			}

			this.FOutput.SliceCount = validcnt;
            for (int i = 0; i < validcnt; i++)
            {
                if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<DX11IndexedGeometry>(); }
            }

            this.FOutput.Stream.IsChanged = true;
		}

        public void Update(DX11RenderContext context)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                if (this.FOutput[i].Contains(context)) { this.FOutput[i].Dispose(context); }
            }
            
            if (this.FBodies.SliceCount > 0)
            {
                int cnt = this.FBodies.SliceCount;

                for (int i = 0; i < cnt; i++)
                {
                    SoftBody body = this.FBodies[i];

                    SoftBodyCustomData sc = (SoftBodyCustomData)body.UserObject;

                    AlignedFaceArray faces = body.Faces;

                    if (FValid[i])
                    {
                        if (body.Faces.Count > 0)
                        {
                            #region Build from Faces
                            DX11IndexedGeometry geom = new DX11IndexedGeometry(context);

                            geom.VerticesCount = faces.Count*3;


                            if (sc.HasUV)
                            {
                                geom.InputLayout = Pos3Norm3Tex2Vertex.Layout;
                                geom.VertexSize = Pos3Norm3Tex2Vertex.VertexSize;
                            }
                            else
                            {
                                geom.InputLayout = Pos3Norm3Vertex.Layout;
                                geom.VertexSize = Pos3Norm3Vertex.VertexSize;
                            }

                            //Mesh mesh = new Mesh(OnDevice, faces.Count, faces.Count * 3, MeshFlags.SystemMemory | MeshFlags.Use32Bit, decl);

                            SlimDX.DataStream verts = new SlimDX.DataStream(geom.VerticesCount * geom.VertexSize*3, false, true);
                            SlimDX.DataStream indices = new SlimDX.DataStream(faces.Count * sizeof(int)*3, false, true);

                            int j;
                            int uvcnt = 0;
                            for (j = 0; j < faces.Count; j++)
                            {
                                NodePtrArray nodes = faces[j].N;
                                verts.Write(nodes[0].X);
                                verts.Write(nodes[0].Normal);
                                //verts.Position += 12;

                                if (sc.HasUV)
                                {
                                    verts.Write(sc.UV[uvcnt]);
                                    uvcnt++;
                                    verts.Write(sc.UV[uvcnt]);
                                    uvcnt++;
                                }

                                verts.Write(nodes[1].X);
                                verts.Write(nodes[1].Normal);

                                //verts.Position += 12;

                                if (sc.HasUV)
                                {
                                    verts.Write(sc.UV[uvcnt]);
                                    uvcnt++;
                                    verts.Write(sc.UV[uvcnt]);
                                    uvcnt++;
                                }

                                verts.Write(nodes[2].X);
                                verts.Write(nodes[2].Normal);
                                //verts.Position += 12;

                                if (sc.HasUV)
                                {
                                    verts.Write(sc.UV[uvcnt]);
                                    uvcnt++;
                                    verts.Write(sc.UV[uvcnt]);
                                    uvcnt++;
                                }

                                indices.Write(j * 3);
                                indices.Write(j * 3 + 1);
                                indices.Write(j * 3 + 2);

                            }

                            geom.VertexBuffer = BufferHelper.CreateVertexBuffer(context, verts, false, true);

                            geom.HasBoundingBox = false;
                            geom.Topology = SlimDX.Direct3D11.PrimitiveTopology.TriangleList;
                            DX11IndexBuffer ibo = new DX11IndexBuffer(context, indices,false,true);
                            geom.IndexBuffer = ibo;
                            this.FOutput[i][context] = geom;
                            #endregion
                        }
                    }
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                if (this.FOutput[i] != null)
                {
                    this.FOutput[i].Dispose(context);
                }
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                if (this.FOutput[i] != null)
                {
                    this.FOutput[i].Dispose();
                    this.FOutput[i] = null;
                }
            }
        }
    }
}
