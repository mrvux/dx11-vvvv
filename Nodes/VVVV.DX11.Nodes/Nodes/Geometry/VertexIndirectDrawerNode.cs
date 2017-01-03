using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "VertexIndirect", Category = "DX11.Drawer", Version = "", Author = "vux")]
    public class VertexIndirectDrawerNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In", CheckIfChanged = true)]
        protected Pin<DX11Resource<DX11VertexGeometry>> FInGeom;

        [Input("Default Instance Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInCnt;

        [Input("Vertex Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11RWResource>> FInV;

        [Input("Instance Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11RWResource>> FInI;

        [Input("Enabled",DefaultValue=1)]
        protected IDiffSpread<bool> FInEnabled;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<DX11VertexGeometry>> FOutGeom;

        bool invalidate = false;

        public void Evaluate(int SpreadMax)
        {
            invalidate = false;

            if (this.FInGeom.PluginIO.IsConnected)
            {
                this.FOutGeom.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++) { this.FOutGeom[i] = new DX11Resource<DX11VertexGeometry>(); }

                invalidate = this.FInGeom.IsChanged || this.FInEnabled.IsChanged || this.FInCnt.IsChanged;

            }
            else
            {
                this.FOutGeom.SliceCount = 0;
            }
        }

        public void Update(DX11RenderContext context)
        {
            for (int i = 0; i < this.FOutGeom.SliceCount; i++)
            {
                DX11VertexGeometry geom = (DX11VertexGeometry)this.FInGeom[i][context].ShallowCopy();

                if (this.FInEnabled[i])
                {

                    if (this.FInGeom.IsChanged || this.FInCnt.IsChanged)
                    {
                        if (this.FOutGeom[i].Contains(context))
                        {
                            var g = this.FOutGeom[i][context];
                            DX11VertexIndirectDrawer d = (DX11VertexIndirectDrawer)g.Drawer;

                            if (d != null)
                            {
                                d.IndirectArgs.Dispose();
                            }
                        }

                        DX11VertexIndirectDrawer ind = new DX11VertexIndirectDrawer();
                        geom.AssignDrawer(ind);

                        ind.Update(context, this.FInCnt[i]);
                    }

                    DX11VertexIndirectDrawer drawer = (DX11VertexIndirectDrawer)geom.Drawer;

                    if (this.FInI.IsConnected)
                    {
                        drawer.IndirectArgs.CopyInstanceCount(context.CurrentDeviceContext, this.FInI[i][context].UAV);
                    }

                    if (this.FInV.IsConnected)
                    {
                        drawer.IndirectArgs.CopyVertexCount(context.CurrentDeviceContext, this.FInV[i][context].UAV);
                    }

                    this.FOutGeom[i][context] = geom;
                }
            }
        }

        public void Destroy(DX11RenderContext OnDevice, bool force)
        {
            //Not ownding resource eg: do nothing
        }
    }
}
