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



namespace VVVV.DX11.Text3d
{
    public class OutlineRenderer : SharpDX.DirectWrite.TextRendererBase
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
                    c = sb.Color;
                }
            }

            if (glyphRun.Indices.Length > 0)
            {
                PathGeometry pg = new PathGeometry(this.factory);

                GeometrySink sink = pg.Open();

                glyphRun.FontFace.GetGlyphRunOutline(glyphRun.FontSize, glyphRun.Indices, glyphRun.Advances, glyphRun.Offsets, glyphRun.IsSideways, glyphRun.BidiLevel % 2 == 1, sink as SimplifiedGeometrySink);
                sink.Close();

                TransformedGeometry tg = new TransformedGeometry(this.factory, pg, Matrix3x2.Translation(baselineOriginX, baselineOriginY) * Matrix3x2.Scaling(1.0f, -1.0f));

                pg.Dispose();

                //Transform from baseline

                this.AddGeometry(tg);

                return SharpDX.Result.Ok;
            }
            else
            {
                return SharpDX.Result.Ok;
            }

        }

        public override Result DrawUnderline(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Underline underline, ComObject clientDrawingEffect)
        {
            PathGeometry pg = new PathGeometry(this.factory);
            GeometrySink sink = pg.Open();

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


            TransformedGeometry tg = new TransformedGeometry(this.factory, pg, Matrix3x2.Translation(baselineOriginX, baselineOriginY) * Matrix3x2.Scaling(1.0f, -1.0f));
            pg.Dispose();

            this.AddGeometry(tg);
            return Result.Ok;
        }

        public override Result DrawStrikethrough(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Strikethrough strikethrough, ComObject clientDrawingEffect)
        {
            PathGeometry pg = new PathGeometry(this.factory);
            GeometrySink sink = pg.Open();

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


            TransformedGeometry tg = new TransformedGeometry(this.factory, pg, Matrix3x2.Translation(baselineOriginX, baselineOriginY) * Matrix3x2.Scaling(1.0f, -1.0f));
            pg.Dispose();

            this.AddGeometry(tg);
            return Result.Ok;
        }


        public override SharpDX.Mathematics.Interop.RawMatrix3x2 GetCurrentTransform(object clientDrawingContext)
        {
            return SharpDX.Matrix3x2.Identity;
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

                GeometrySink sink = pg.Open();

                this.geometry.Combine(geom, CombineMode.Union, sink);

                sink.Close();

                this.geometry = pg;
            }
        }
    }

}
