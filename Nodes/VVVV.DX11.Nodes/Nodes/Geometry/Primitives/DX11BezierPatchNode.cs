using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V2;

using SlimDX;

using FeralTic.Core.Maths;
using FeralTic.DX11.Geometry;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "BezierPatch", Category = "DX11.Geometry", Version = "2d", Author = "vux")]
    public class BezierPatchNode : DX11AbstractMeshNode, IPluginEvaluate
    {
        #region Fields
        private ITransformIn FPinInTransform;

        private IValueIn FPInInCtrlPts;
        private IValueIn FPInInRes;
        private IValueIn FPinInCtrlRes;
        private IValueIn FPinInMeshCount;
        private IValueIn FPinInAbsolute;

        private IValueOut FPinOutHelpers;
        private IValueOut FOutPatchId;
        #endregion

        [ImportingConstructor()]
        public BezierPatchNode(IPluginHost Host) : base(Host) { }

        #region Set Plugin Host
        protected override void SetInputs()
        {
            this.FHost.CreateTransformInput("Transform In", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInTransform);

            this.FHost.CreateValueInput("Control Points", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPInInCtrlPts);
            this.FPInInCtrlPts.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueInput("Control Point Resolution", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInCtrlRes);
            this.FPinInCtrlRes.SetSubType2D(1, double.MaxValue, 1, 2, 2, false, false, true);

            this.FHost.CreateValueInput("Grid Resolution", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPInInRes);
            this.FPInInRes.SetSubType2D(2, double.MaxValue, 1, 2, 2, false, false, true);

            this.FHost.CreateValueInput("Mesh Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInMeshCount);
            this.FPinInMeshCount.SetSubType(double.MinValue, double.MaxValue, 1, 1, false, false, true);

            this.FHost.CreateValueInput("Absolute Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInAbsolute);
            this.FPinInAbsolute.SetSubType(0, 1, 1, 0, false, true, false);

            this.FHost.CreateValueOutput("Helpers", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutHelpers);
            this.FPinOutHelpers.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);
            this.FPinOutHelpers.Order = 20;

            this.FHost.CreateValueOutput("Patch Id", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FOutPatchId);
            this.FOutPatchId.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, true);
            this.FOutPatchId.Order = 21;
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (this.FPInInCtrlPts.PinIsChanged || this.FPInInRes.PinIsChanged 
                || this.FPinInMeshCount.PinIsChanged || this.FPinInCtrlRes.PinIsChanged || this.FPinInAbsolute.PinIsChanged)
            {
                this.FVertex.Clear();
                this.FIndices.Clear();

                List<double> hx = new List<double>();
                List<double> hy = new List<double>();
                List<int> pid = new List<int>();

                double dblx, dbly;

                double mc;
                this.FPinInMeshCount.GetValue(0, out mc);

                int patchcnt = (int)mc;

                double dabs;
                this.FPinInAbsolute.GetValue(0, out dabs);

                bool useAbsolute = dabs >= 0.5;

                int ctrlidx = 0;

                for (int pc = 0; pc < patchcnt; pc++)
                {

                    this.FPInInRes.GetValue2D(pc, out dblx, out dbly);

                    int resX = Convert.ToInt32(dblx);
                    int resY = Convert.ToInt32(dbly);


                    this.FPinInCtrlRes.GetValue2D(pc, out dblx, out dbly);
                    int CresX = Convert.ToInt32(dblx);
                    int CresY = Convert.ToInt32(dbly);

                    List<Pos4Norm3Tex2Vertex> verts = new List<Pos4Norm3Tex2Vertex>();

                    float sx = 0.5f;
                    float sy = 0.5f;

                    float ix = (sx / Convert.ToSingle(resX - 1)) * 2.0f;
                    float iy = (sy / Convert.ToSingle(resY - 1)) * 2.0f;


                    float y = -sy;


                    Pos4Norm3Tex2Vertex v = new Pos4Norm3Tex2Vertex();
                    v.Normals = new Vector3(0, 0, -1.0f);

                    List<Vector3> ctrls = new List<Vector3>();

                    float mx = -0.5f;
                    float my = 0.5f;

                    float incx = 1.0f / ((float)CresX - 1.0f);
                    float incy = 1.0f / ((float)CresY - 1.0f);

                    int inch = 0;

                    double cx, cy;
                    for (int ct = 0; ct < CresX * CresY; ct++)
                    {
                        this.FPInInCtrlPts.GetValue2D(ct + ctrlidx, out cx, out cy);

                        ctrls.Add(new Vector3((float)cx, (float)cy, 0.0f));
                    }


                    Matrix4x4 mat;
                    this.FPinInTransform.GetMatrix(pc, out mat);



                    for (int ct = 0; ct < CresX * CresY; ct++)
                    {
                        this.FPInInCtrlPts.GetValue2D(ct + ctrlidx, out cx, out cy);

                        ctrls.Add(new Vector3((float)cx, (float)cy, 0.0f));

                        Vector2D vd = new Vector2D(cx + mx, cy + my);
                        Vector3D v2 = mat * vd;

                        hx.Add(v2.x);
                        hy.Add(v2.y);

                        mx += incx;

                        inch++;
                        if (inch == CresX)
                        {
                            inch = 0;
                            mx = -0.5f;
                            my -= incy;
                        }

                        pid.Add(pc);
                    }

                    Vector3[] carr = new Vector3[ctrls.Count];

                    for (int i = 0; i < resY; i++)
                    {
                        float x = -sx;
                        for (int j = 0; j < resX; j++)
                        {
                            //v.Position = new Vector4(x, y, 0.0f, 1.0f);
                            float tu1 = Convert.ToSingle(VMath.Map(j, 0, resX - 1, 0.0, 1.0, TMapMode.Clamp));
                            float tv1 = Convert.ToSingle(VMath.Map(i, 0, resY - 1, 1.0, 0.0, TMapMode.Clamp));
                            v.TexCoords = new Vector2(tu1, tv1);

                            float[] bu = BernsteinBasis.ComputeBasis(CresX -1,tu1);
                            float[] bv = BernsteinBasis.ComputeBasis(CresY -1,tv1);

                            if (useAbsolute)
                            {
                                for (int ck = 0; ck < ctrls.Count; ck++)
                                {
                                    carr[ck].X = ctrls[ck].X;
                                    carr[ck].Y = ctrls[ck].Y;
                                }
                            }
                            else
                            {
                                for (int ck = 0; ck < ctrls.Count; ck++)
                                {
                                    carr[ck].X = x + ctrls[ck].X;
                                    carr[ck].Y = y + ctrls[ck].Y;
                                }
                            }


                            Vector3 vp = this.EvaluateBezier(carr, bu, bv, CresX, CresY);

                            v.Position = new Vector4(vp.X, vp.Y, 0.0f,1.0f);
                            x += ix;


                            //ds.Write<PosNormTexVertex>(v);
                            verts.Add(v);
                        }
                        y += iy;
                    }

                    this.FVertex.Add(verts.ToArray());

                    List<int> indlist = new List<int>();
                    for (int j = 0; j < resY - 1; j++)
                    {
                        int rowlow = (j * resX);
                        int rowup = ((j + 1) * resX);
                        for (int i = 0; i < resX - 1; i++)
                        {

                            int col = i * (resX - 1);

                            indlist.Add(0 + rowlow + i);                  
                            indlist.Add(0 + rowup + i);
                            indlist.Add(1 + rowlow + i);


                            
                            indlist.Add(1 + rowup + i);
                            indlist.Add(1 + rowlow + i);
                            indlist.Add(0 + rowup + i);
                        }
                    }

                    this.FIndices.Add(indlist.ToArray());

                    ctrlidx += CresX * CresY;
                }
               

                this.FPinOutHelpers.SliceCount = hx.Count;

                for (int i = 0; i < hx.Count; i++)
                {
                    this.FPinOutHelpers.SetValue2D(i, hx[i], hy[i]);
                }

                this.FOutPatchId.SliceCount = pid.Count;
                for (int i = 0; i < pid.Count; i++)
                {
                    this.FOutPatchId.SetValue(i, pid[i]);
                }

                this.InvalidateMesh(patchcnt);
            }
        }
        #endregion

        Vector3 EvaluateBezier(Vector3[] verts,
                       float[] BasisU,
                       float[] BasisV,int cx, int cy)
        {
            
            Vector3 Value = Vector3.Zero;
            int cnt = 0;
            for (int i = 0; i < cy; i++)
            {
                Vector3 vl = Vector3.Zero;
                for (int j = 0; j < cx; j++)
                {
                    vl += verts[cnt] * BasisU[j];
                    cnt ++;
                }
                vl *= BasisV[i];

                Value += vl;
            }
            return Value;
        }
    }


}
