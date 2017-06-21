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

namespace VVVV.DX11.Nodes.Geometry
{
    [PluginInfo(Name = "NullIndirect", Category = "DX11.Drawer", Version = "", Author = "vux")]
    public class NullIndirectDrawerNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Default Vertex Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInVCnt;

        [Input("Default Instance Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInICnt;

        [Input("Vertex Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11RWResource>> FInV;

        [Input("Instance Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11RWResource>> FInI;

        [Input("Enabled")]
        protected IDiffSpread<bool> FInEnabled;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<DX11NullGeometry>> FOutGeom;

        bool invalidate = false;

        public void Evaluate(int SpreadMax)
        {
            invalidate = false;

            this.FOutGeom.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutGeom[i] == null)
                {
                    this.FOutGeom[i] = new DX11Resource<DX11NullGeometry>();
                }
            }

            invalidate = this.FInICnt.IsChanged || this.FInEnabled.IsChanged || this.FInVCnt.IsChanged;
        }

        public void Update(DX11RenderContext context)
        {
            for (int i = 0; i < this.FOutGeom.SliceCount; i++)
            {
                if (this.FInEnabled[i])
                {
                    if (this.FInVCnt.IsChanged || this.FInICnt.IsChanged || this.FOutGeom[i].Contains(context) == false)
                    {
                        if (this.FOutGeom[i].Contains(context))
                        {
                            this.FOutGeom[i].Dispose(context);
                            

                        }

                        this.FOutGeom[i][context] = new DX11NullGeometry(context);
                        DX11NullIndirectDrawer ind = new DX11NullIndirectDrawer();
                        ind.Update(context, this.FInVCnt[i], this.FInICnt[i]);
                        this.FOutGeom[i][context].AssignDrawer(ind);
                    }

                    DX11NullIndirectDrawer drawer = (DX11NullIndirectDrawer)this.FOutGeom[i][context].Drawer;

                    if (this.FInI.IsConnected)
                    {
                        drawer.IndirectArgs.CopyInstanceCount(context.CurrentDeviceContext, this.FInI[i][context].UAV);
                    }

                    if (this.FInV.IsConnected)
                    {
                        drawer.IndirectArgs.CopyVertexCount(context.CurrentDeviceContext, this.FInV[i][context].UAV);
                    }
                }
            }
        }

        public void Destroy(DX11RenderContext OnDevice, bool force)
        {
            //Not ownding resource eg: do nothing
        }
    }
}
