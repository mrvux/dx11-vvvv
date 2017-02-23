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
    [PluginInfo(Name = "PerVertexDrawer", Category = "DX11.Drawer", Version = "", Author = "vux")]
    public class DX11PerVertexIndexDrawerNode : IPluginEvaluate, IDX11ResourceHost
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

            if (this.FInGeom.PluginIO.IsConnected)
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

            for (int i = 0; i < this.FOutGeom.SliceCount; i++)
            {
                if (this.FInEnabled[i] && this.FInGeom[i].Contains(context))
                {
                    IDX11Geometry g = this.FInGeom[i][context];

                    if (g != null)
                    {
                        if (g is DX11IndexedGeometry)
                        {
                            DX11IndexedGeometry v = (DX11IndexedGeometry)this.FInGeom[i][context].ShallowCopy();

                            DX11PerVertexIndexedDrawer drawer = new DX11PerVertexIndexedDrawer();
                            v.AssignDrawer(drawer);

                            this.FOutGeom[i][context] = v;
                        }
                        else
                        {
                            this.FOutGeom[i][context] = g;
                        }
                    }
                }
                else
                {
                    this.FOutGeom[i][context] = this.FInGeom[i][context];
                }

            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            //Not ownding resource eg: do nothing
        }
    }
}
