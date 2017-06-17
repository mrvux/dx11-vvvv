using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DWriteFactory = SharpDX.DirectWrite.Factory;
using D2DFactory = SharpDX.Direct2D1.Factory;
using D2DGeometry = SharpDX.Direct2D1.Geometry;
using SharpDX.DirectWrite;
using SharpDX;

using RawMat = SharpDX.Mathematics.Interop.RawMatrix3x2;



namespace VVVV.DX11.Text3d
{
    public unsafe class OutlineRenderer : SharpDX.DirectWrite.TextRendererBase
    {
        private readonly D2DFactory factory;
        private SharpDX.Direct2D1.Geometry geometry = null;

        public OutlineRenderer(D2DFactory factory)
        {
            this.factory = factory;
        }

        public override SharpDX.Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, SharpDX.ComObject clientDrawingEffect)
        {
            Color4 c = Color4.White;
            if (clientDrawingEffect != null)
            {
                if (clientDrawingEffect is SharpDX.Direct2D1.SolidColorBrush)
                {
                    var sb = (SharpDX.Direct2D1.SolidColorBrush)clientDrawingEffect;
                    SharpDX.Mathematics.Interop.RawColor4 brushColor = sb.Color;
                    c = *(Color4*)&brushColor;
                }
            }

            if (glyphRun.Indices.Length > 0)
            {
                using (PathGeometry pg = new PathGeometry(this.factory))
                {
                    using (GeometrySink sink = pg.Open())
                    {
                        glyphRun.FontFace.GetGlyphRunOutline(glyphRun.FontSize, glyphRun.Indices, glyphRun.Advances, glyphRun.Offsets, glyphRun.IsSideways, glyphRun.BidiLevel % 2 == 1, sink as SimplifiedGeometrySink);
                        sink.Close();


                        Matrix3x2 mat = Matrix3x2.Translation(baselineOriginX, baselineOriginY) * Matrix3x2.Scaling(1.0f, -1.0f);
                        TransformedGeometry tg = new TransformedGeometry(this.factory, pg, *(RawMat*)&mat);
                        this.AddGeometry(tg);
                    }
                }
                return SharpDX.Result.Ok;
            }
            else
            {
                return SharpDX.Result.Ok;
            }

        }

        public override Result DrawUnderline(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Underline underline, ComObject clientDrawingEffect)
        {
            using (PathGeometry pg = new PathGeometry(this.factory))
            {
                using (GeometrySink sink = pg.Open())
                {
                    Vector2 topLeft = new Vector2(0.0f, underline.Offset);
                    sink.BeginFigure(topLeft, FigureBegin.Filled);
                    topLeft.X += underline.Width;
                    sink.AddLine(topLeft);
                    topLeft.Y += underline.Thickness;
                    sink.AddLine(topLeft);
                    topLeft.X -= underline.Width;
                    sink.AddLine(topLeft);
                    sink.EndFigure(FigureEnd.Closed);
                    sink.Close();

                    Matrix3x2 mat = Matrix3x2.Translation(baselineOriginX, baselineOriginY) * Matrix3x2.Scaling(1.0f, -1.0f);
                    TransformedGeometry tg = new TransformedGeometry(this.factory, pg, *(RawMat*)&mat);

                    this.AddGeometry(tg);
                    return Result.Ok;
                }
            }
        }

        public override Result DrawStrikethrough(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Strikethrough strikethrough, ComObject clientDrawingEffect)
        {
            using (PathGeometry pg = new PathGeometry(this.factory))
            {
                using (GeometrySink sink = pg.Open())
                {
                    Vector2 topLeft = new Vector2(0.0f, strikethrough.Offset);
                    sink.BeginFigure(topLeft, FigureBegin.Filled);
                    topLeft.X += strikethrough.Width;
                    sink.AddLine(topLeft);
                    topLeft.Y += strikethrough.Thickness;
                    sink.AddLine(topLeft);
                    topLeft.X -= strikethrough.Width;
                    sink.AddLine(topLeft);
                    sink.EndFigure(FigureEnd.Closed);
                    sink.Close();

                    Matrix3x2 mat = Matrix3x2.Translation(baselineOriginX, baselineOriginY) * Matrix3x2.Scaling(1.0f, -1.0f);
                    TransformedGeometry tg = new TransformedGeometry(this.factory, pg, *(RawMat*)&mat);

                    this.AddGeometry(tg);
                    return Result.Ok;
                }
            }
        }

        public override SharpDX.Mathematics.Interop.RawMatrix3x2 GetCurrentTransform(object clientDrawingContext)
        {
            return new SharpDX.Mathematics.Interop.RawMatrix3x2()
            {
                M11 = 1.0f,
                M12 = 0.0f,
                M21 = 0.0f,
                M22 = 1.0f,
                M31 = 0.0f,
                M32 = 0.0f
            };
        }

        public override bool IsPixelSnappingDisabled(object clientDrawingContext)
        {
            return true;
        }

        public override float GetPixelsPerDip(object clientDrawingContext)
        {
            return 1.0f;
        }

        public SharpDX.Direct2D1.Geometry GetGeometry()
        {
            return this.geometry;
        }

        protected void AddGeometry(D2DGeometry geom)
        {
            if (this.geometry == null)
            {
                this.geometry = geom;
            }
            else
            {
                PathGeometry pg = new PathGeometry(this.factory);

                using (GeometrySink sink = pg.Open())
                {
                    this.geometry.Combine(geom, CombineMode.Union, sink);
                    sink.Close();
                }
                var oldGeom = this.geometry;
                this.geometry = pg;
                oldGeom.Dispose();

            }
        }
    }

}
