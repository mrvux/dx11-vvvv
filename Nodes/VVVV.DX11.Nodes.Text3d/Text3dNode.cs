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
    [PluginInfo(Name = "Text", Category = "DX11.Geometry", Version = "", Author = "vux", Help ="Builds 3d text geometry", Warnings ="Please note that there are no UV for the text")]
    public unsafe class TextMeshNode : DX11BaseVertexPrimitiveNode
    {
        [Input("Text", DefaultString = "DX11")]
        protected IDiffSpread<string> FText;

        [Input("Font", EnumName = "DirectWrite_Font_Families")]
        protected IDiffSpread<EnumEntry> FFontInput;

        [Input("Font Size", DefaultValue = 32)]
        protected IDiffSpread<int> FFontSize;

        [Input("Horizontal Aligment")]
        protected IDiffSpread<TextAlignment> FHAlignment;

        [Input("Vertical Aligment")]
        protected IDiffSpread<ParagraphAlignment> FVAlignment;

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

            TextFormat fmt = new TextFormat(dwFactory, this.FFontInput[slice].Name, FFontSize[slice]);
            TextLayout tl = new TextLayout(dwFactory, FText[slice], fmt, 0.0f, 32.0f);

            tl.WordWrapping = WordWrapping.NoWrap;
            tl.TextAlignment = FHAlignment[slice];
            tl.ParagraphAlignment = FVAlignment[slice];

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

            for (int i = 0; i < vertexList.Count;i++)
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
            fmt.Dispose();
            tl.Dispose();

            return vg;
        }

        protected override bool Invalidate()
        {
            bool b = false;

            b = b || this.FText.IsChanged || this.FExtrude.IsChanged || this.FFontSize.IsChanged
                || this.FFontInput.IsChanged || this.FHAlignment.IsChanged || this.FVAlignment.IsChanged;

            return b;

        }
    }
}
