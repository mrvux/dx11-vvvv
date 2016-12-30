using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using SlimDX;

using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using FeralTic.DX11.Geometry;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Polygon", Category = "DX11.Geometry", Version = "2d", Author = "vux")]
    public class Polygon2dNode : DX11AbstractMeshNode,IPluginEvaluate
    {
        #region Fields
        private IValueIn FPinInVertices;
        private IValueIn FPinInVerticesCount;
        #endregion

        [ImportingConstructor()]
        public Polygon2dNode(IPluginHost Host) : base(Host) {}

        #region Set Plugin Host
        protected override void SetInputs()
        {
            this.FHost.CreateValueInput("Vertices", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVertices);
            this.FPinInVertices.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueInput("Vertex Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVerticesCount);
            this.FPinInVerticesCount.SetSubType(3, double.MaxValue, 1, 3, false, false, true);
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (this.FPinInVertices.PinIsChanged || this.FPinInVerticesCount.PinIsChanged)
            {
                this.FVertex.Clear();
                this.FIndices.Clear();
                int cnt = 0;
                for (int i = 0; i < this.FPinInVerticesCount.SliceCount; i++)
                {
                    double dblcount;
                    this.FPinInVerticesCount.GetValue(i, out dblcount);

                    if (dblcount >= 3)
                    {
                        double cx = 0;
                        double cy = 0;
                        double x, y;

                        double minx = double.MaxValue, miny = double.MaxValue;
                        double maxx = double.MinValue, maxy = double.MinValue;

                        Pos4Norm3Tex2Vertex[] verts = new Pos4Norm3Tex2Vertex[Convert.ToInt32(dblcount) + 1];

                        for (int j = 0; j < dblcount; j++)
                        {
                            this.FPinInVertices.GetValue2D(cnt,out x,out y);
                            verts[j + 1].Position = new Vector4(Convert.ToSingle(x), Convert.ToSingle(y), 0,1.0f);
                            verts[j + 1].Normals = new Vector3(0, 0, 1);
                            verts[j+1].TexCoords = new Vector2(0.0f,0.0f);
                            cx += x;
                            cy += y;

                            if (x < minx) { minx = x; }
                            if (x > maxx) { maxx = x; }
                            if (y < miny) { miny = y; }
                            if (y > maxy) { maxy = y; }

                            cnt++;
                        }

                        verts[0].Position = new Vector4(Convert.ToSingle(cx / dblcount), Convert.ToSingle(cy / dblcount), 0, 1.0f);
                        verts[0].Normals = new Vector3(0, 0, 1);

                        double w = maxx - minx;
                        double h = maxy - miny;
                        for (int j = 0; j <= dblcount; j++)
                        {
                            verts[j].TexCoords = new Vector2(Convert.ToSingle((verts[j].Position.X - minx) / w),
                                 Convert.ToSingle((verts[j].Position.Y - miny) / h));
                        }

                        this.FVertex.Add(verts);

                        List<int> inds = new List<int>();

                        for (int j = 0; j < dblcount - 1; j++)
                        {
                            inds.Add(0);
                            inds.Add(j + 1);
                            inds.Add(j + 2);
                        }

                        inds.Add(0);
                        inds.Add(verts.Length - 1);
                        inds.Add(1);

                        this.FIndices.Add(inds.ToArray());
                    }
                }
                this.InvalidateMesh(this.FVertex.Count);
            }
        }
        #endregion

    }
        
        
}
