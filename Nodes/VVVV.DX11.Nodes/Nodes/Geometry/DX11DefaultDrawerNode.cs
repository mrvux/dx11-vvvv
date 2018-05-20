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
    [PluginInfo(Name = "DefaultDrawer", Category = "DX11.Drawer", Version = "", Author = "vux")]
    public class DX11DefaultIndexDrawerNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In", CheckIfChanged = true)]
        protected Pin<DX11Resource<IDX11Geometry>> FInGeom;

        [Input("Enabled",DefaultValue=1)]
        protected IDiffSpread<bool> FInEnabled;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<IDX11Geometry>> FOutGeom;

        bool invalidate = false;

        public void Evaluate(int SpreadMax)
        {
            invalidate = false;

            if (this.FInGeom.IsConnected)
            {
                this.FOutGeom.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++) { if (this.FOutGeom[i] == null) { this.FOutGeom[i] = new DX11Resource<IDX11Geometry>(); } }

                invalidate = this.FInGeom.IsChanged || this.FInEnabled.IsChanged;

                if (invalidate) { this.FOutGeom.Stream.IsChanged = true; }
            }
            else
            {
                this.FOutGeom.SliceCount = 0;
            }
        }

        public void Update(DX11RenderContext context)
        {
            Device device = context.Device;

            if (invalidate)
            {
                for (int i = 0; i < this.FOutGeom.SliceCount; i++)
                {
                    if (this.FInEnabled[i])
                    {

                        IDX11Geometry copy = this.FInGeom[i][context].ShallowCopy();
                        if (copy is DX11IndexedGeometry)
                        {
                            DX11DefaultIndexedDrawer drawer = new DX11DefaultIndexedDrawer();
                            ((DX11IndexedGeometry)copy).AssignDrawer(drawer);
                        }
                        else if (copy is DX11VertexGeometry)
                        {
                            DX11DefaultVertexDrawer drawer = new DX11DefaultVertexDrawer();
                            ((DX11VertexGeometry)copy).AssignDrawer(drawer);
                        }

                        this.FOutGeom[i][context] = copy;

                        

                    }
                    else
                    {
                        this.FOutGeom[i][context] = this.FInGeom[i][context];
                    }

                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            //Not ownding resource eg: do nothing
        }
    }
}
