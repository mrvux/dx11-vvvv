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
    [PluginInfo(Name = "BezierPatch", Category = "DX11.Geometry", Version = "3d", Author = "vux")]
    public class BezierPatch3dNode : DX11AbstractMeshNode, IPluginEvaluate
    {
        #region Fields
        private IValueIn FPInInCtrlPts;
        private IValueIn FPInInRes;
        private IValueIn FPinInCtrlRes;
        private IValueIn FPinInMeshCount;
        #endregion

        [ImportingConstructor()]
        public BezierPatch3dNode(IPluginHost Host) : base(Host) { }

        #region Set Plugin Host
        protected override void SetInputs()
        {
            this.FHost.CreateValueInput("Control Points", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPInInCtrlPts);
            this.FPInInCtrlPts.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0,0, false, false, false);

            this.FHost.CreateValueInput("Control Point Resolution", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInCtrlRes);
            this.FPinInCtrlRes.SetSubType2D(1, double.MaxValue, 1, 2, 2, false, false, true);

            this.FHost.CreateValueInput("Grid Resolution", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPInInRes);
            this.FPInInRes.SetSubType2D(2, double.MaxValue, 1, 2, 2, false, false, true);

            this.FHost.CreateValueInput("Mesh Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInMeshCount);
            this.FPinInMeshCount.SetSubType(double.MinValue, double.MaxValue, 1, 1, false, false, true);
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (this.FPInInCtrlPts.PinIsChanged || this.FPInInRes.PinIsChanged 
                || this.FPinInMeshCount.PinIsChanged || this.FPinInCtrlRes.PinIsChanged)
            {
                this.FVertex.Clear();
                this.FIndices.Clear();

                double dblx, dbly;

                double mc;
                this.FPinInMeshCount.GetValue(0, out mc);

                int patchcnt = (int)mc;


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

                    double cx, cy,cz;
                    for (int ct = 0; ct < CresX * CresY; ct++)
                    {
                        this.FPInInCtrlPts.GetValue3D(ct + ctrlidx, out cx, out cy, out cz);

                        ctrls.Add(new Vector3((float)cx, (float)cy, (float)cz));
                    }

                    Vector3[] carr = new Vector3[ctrls.Count];

                    for (int i = 0; i < resY; i++)
                    {
                        float x = -sx;
                        for (int j = 0; j < resX; j++)
                        {
                            float tu1 = Convert.ToSingle(VMath.Map(j, 0, resX - 1, 0.0, 1.0, TMapMode.Clamp));
                            float tv1 = Convert.ToSingle(VMath.Map(i, 0, resY - 1, 1.0, 0.0, TMapMode.Clamp));
                            v.TexCoords = new Vector2(tu1, tv1);

                            float[] bu = BernsteinBasis.ComputeBasis(CresX -1,tu1);
                            float[] bv = BernsteinBasis.ComputeBasis(CresY -1,tv1);


                            for (int ck = 0; ck < ctrls.Count; ck++)
                            {
                                carr[ck].X = x + ctrls[ck].X;
                                carr[ck].Y = y + ctrls[ck].Y;
                                carr[ck].Z = ctrls[ck].Z;
                            }

                            Vector3 vp = this.EvaluateBezier(carr, bu, bv, CresX, CresY);

                            v.Position = new Vector4(vp.X, vp.Y, vp.Z,1.0f);
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
