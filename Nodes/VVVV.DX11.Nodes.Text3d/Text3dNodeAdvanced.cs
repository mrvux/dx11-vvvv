using FeralTic.DX11;
using FeralTic.DX11.Resources;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;
using DWriteFactory = SharpDX.DirectWrite.Factory;
using D2DFactory = SharpDX.Direct2D1.Factory;
using D2DGeometry = SharpDX.Direct2D1.Geometry;


using SharpDX.Direct2D1;
using FeralTic.DX11.Geometry;
using SharpDX;
using System.Runtime.InteropServices;

using InputElement = SlimDX.Direct3D11.InputElement;
using VVVV.DX11.Text3d;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Text", Category = "DX11.Geometry", Version = "Advanced", Author = "vux", Help ="Builds 3d text geometry from a text layout", Warnings ="Please note that there are no UV for the text")]
    public unsafe class TextMeshAdvancedNode : DX11BaseVertexPrimitiveNode
    {
        [Input("Input", DefaultString = "DX11")]
        protected Pin<SlimDX.DirectWrite.TextLayout> FTextLayout;

        [Input("Extrude Amount", DefaultValue = 1.0)]
        protected IDiffSpread<float> FExtrude;

        private static SharpDX.Direct2D1.Factory d2dFactory;
        private static SharpDX.DirectWrite.Factory dwFactory;

        private List<Pos3Norm3VertexSDX> vertexList = new List<Pos3Norm3VertexSDX>(1024);

        protected override DX11VertexGeometry GetGeom(DX11RenderContext device, int slice)
        {
            if (d2dFactory == null)
            {
                d2dFactory = new D2DFactory();
                dwFactory = new DWriteFactory(SharpDX.DirectWrite.FactoryType.Shared);
            }

            var textLayout = this.FTextLayout[slice];
            if (textLayout != null)
            {
                TextLayout tl = new TextLayout(textLayout.ComPointer);

                OutlineRenderer renderer = new OutlineRenderer(d2dFactory);
                Extruder ex = new Extruder(d2dFactory);


                tl.Draw(renderer, 0.0f, 0.0f);

                var outlinedGeometry = renderer.GetGeometry();
                ex.GetVertices(outlinedGeometry, vertexList, this.FExtrude[slice]);
                outlinedGeometry.Dispose();

                Vector3 min = new Vector3(float.MaxValue);
                Vector3 max = new Vector3(float.MinValue);

                for (int i = 0; i < vertexList.Count; i++)
                {
                    Pos3Norm3VertexSDX pn = vertexList[i];

                    min.X = pn.Position.X < min.X ? pn.Position.X : min.X;
                    min.Y = pn.Position.Y < min.Y ? pn.Position.Y : min.Y;
                    min.Z = pn.Position.Z < min.Z ? pn.Position.Z : min.Z;

                    max.X = pn.Position.X > max.X ? pn.Position.X : max.X;
                    max.Y = pn.Position.Y > max.Y ? pn.Position.Y : max.Y;
                    max.Z = pn.Position.Z > max.Z ? pn.Position.Z : max.Z;
                }

                SlimDX.DataStream ds = new SlimDX.DataStream(vertexList.Count * Pos3Norm3VertexSDX.VertexSize, true, true);
                ds.Position = 0;

                for (int i = 0; i < vertexList.Count; i++)
                {
                    ds.Write(vertexList[i]);
                }

                ds.Position = 0;

                var vbuffer = new SlimDX.Direct3D11.Buffer(device.Device, ds, new SlimDX.Direct3D11.BufferDescription()
                {
                    BindFlags = SlimDX.Direct3D11.BindFlags.VertexBuffer,
                    CpuAccessFlags = SlimDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SlimDX.Direct3D11.ResourceOptionFlags.None,
                    SizeInBytes = (int)ds.Length,
                    Usage = SlimDX.Direct3D11.ResourceUsage.Default
                });

                ds.Dispose();

                DX11VertexGeometry vg = new DX11VertexGeometry(device);
                vg.InputLayout = Pos3Norm3VertexSDX.Layout;
                vg.Topology = SlimDX.Direct3D11.PrimitiveTopology.TriangleList;
                vg.VertexBuffer = vbuffer;
                vg.VertexSize = Pos3Norm3VertexSDX.VertexSize;
                vg.VerticesCount = vertexList.Count;
                vg.HasBoundingBox = true;
                vg.BoundingBox = new SlimDX.BoundingBox(new SlimDX.Vector3(min.X, min.Y, min.Z), new SlimDX.Vector3(max.X, max.Y, max.Z));

                renderer.Dispose();
                return vg;
            }
            else
            {
                return null;
            }

            
            
        }

        protected override bool Invalidate()
        {
            bool b = false;

            b = b || this.FTextLayout.IsChanged || this.FExtrude.IsChanged;

            return b;

        }
    }
}
