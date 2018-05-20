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
    [PluginInfo(Name = "VertexInstancer", Category = "DX11.Drawer", Version = "", Author = "vux")]
    public class VertexInstancedDrawerNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In", CheckIfChanged = true)]
        protected Pin<DX11Resource<DX11VertexGeometry>> FInGeom;

        [Input("Instance Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInCnt;

        [Input("Enabled",DefaultValue=1)]
        protected IDiffSpread<bool> FInEnabled;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<DX11VertexGeometry>> FOutGeom;

        bool invalidate = false;

        public void Evaluate(int SpreadMax)
        {
            invalidate = false;

            if (this.FInGeom.IsConnected)
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
                    DX11InstancedVertexDrawer d = new DX11InstancedVertexDrawer();
                    d.InstanceCount = this.FInCnt[i];

                    geom.AssignDrawer(d);
                }

                this.FOutGeom[i][context] = geom;
            }
        }

        public void Destroy(DX11RenderContext OnDevice, bool force)
        {
            //Not ownding resource eg: do nothing
        }
    }
}
