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
    [PluginInfo(Name = "IndexedInstancer", Category = "DX11.Drawer", Version = "", Author = "vux")]
    public class IndexedInstancedDrawerNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In", CheckIfChanged = true)]
        protected Pin<DX11Resource<DX11IndexedGeometry>> FInGeom;

        [Input("Instance Count", DefaultValue=1)]
        protected IDiffSpread<int> FInCnt;

        [Input("Start Index Location", DefaultValue = 0,Visibility=PinVisibility.OnlyInspector)]
        protected IDiffSpread<int> FInSI;

        [Input("Base Vertex Location", DefaultValue = 0,Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<int> FInVL;

        [Input("Start Instance Location", DefaultValue = 0,Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<int> FInSL;

        [Input("Enabled",DefaultValue=1)]
        protected IDiffSpread<bool> FInEnabled;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<DX11IndexedGeometry>> FOutGeom;

        bool invalidate = false;

        public void Evaluate(int SpreadMax)
        {
            invalidate = false;

            if (this.FInGeom.PluginIO.IsConnected)
            {
                this.FOutGeom.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++) { this.FOutGeom[i] = new DX11Resource<DX11IndexedGeometry>(); }

                invalidate = this.FInGeom.IsChanged || this.FInEnabled.IsChanged
                    || this.FInCnt.IsChanged || this.FInSI.IsChanged || this.FInSL.IsChanged || this.FInVL.IsChanged;

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
                DX11IndexedGeometry geom = (DX11IndexedGeometry)this.FInGeom[i][context].ShallowCopy();
                if (this.FInEnabled[i])
                {
                    DX11InstancedIndexedDrawer d = new DX11InstancedIndexedDrawer();
                    d.InstanceCount = this.FInCnt[i];
                    d.StartIndexLocation = this.FInSI[0];
                    d.StartInstanceLocation = this.FInSL[0];
                    d.BaseVertexLocation = this.FInVL[0];

                    geom.AssignDrawer(d);
                    //geom.Topology = this.FInTopology[i];
                }

                this.FOutGeom[i][context] = geom;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            //Not ownding resource eg: do nothing
        }
    }
}
