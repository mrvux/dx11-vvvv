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
using VVVV.DX11.Nodes;
using SharpDX.Direct2D1;

namespace VVVV.DX11.Text3d
{
    public class Extruder
    {
        private D2DFactory factory;

        public Extruder(D2DFactory factory)
        {
            this.factory = factory;
        }

        const float sc_flatteningTolerance = .1f;

        private D2DGeometry FlattenGeometry(D2DGeometry geometry, float tolerance)
        {
            PathGeometry path = new PathGeometry(this.factory);

            using (GeometrySink sink = path.Open())
            {
                geometry.Simplify(GeometrySimplificationOption.Lines, tolerance, sink);
                sink.Close();
            }
            return path;
        }

        private D2DGeometry OutlineGeometry(D2DGeometry geometry)
        {
            PathGeometry path = new PathGeometry(this.factory);

            using (GeometrySink sink = path.Open())
            {
                geometry.Outline(sink);
                sink.Close();
            }

            return path;
        }


        public void GetVertices(D2DGeometry geometry, List<Pos3Norm3VertexSDX> vertices, float height = 24.0f)
        {
            vertices.Clear();
            //Empty mesh
            if (geometry == null)
            {
                Pos3Norm3VertexSDX zero = new Pos3Norm3VertexSDX();
                vertices.Add(zero);
                vertices.Add(zero);
                vertices.Add(zero);
            }

            using (D2DGeometry flattenedGeometry = this.FlattenGeometry(geometry, sc_flatteningTolerance))
            {
                using (D2DGeometry outlinedGeometry = this.OutlineGeometry(flattenedGeometry))
                {
                    using (ExtrudingSink sink = new ExtrudingSink(vertices, height))
                    {
                        outlinedGeometry.Simplify(GeometrySimplificationOption.Lines, sink);
                        outlinedGeometry.Tessellate(sink);
                    }

                }
            }
        }
    }


}
