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
    [PluginInfo(Name = "IndexIndirect", Category = "DX11.Drawer", Version = "", Author = "vux")]
    public class IndexIndirectDrawerNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In", CheckIfChanged = true)]
        protected Pin<DX11Resource<DX11IndexedGeometry>> FInGeom;

        [Input("Default Instance Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInCnt;

        [Input("Index Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11RWResource>> FInIdx;

        [Input("Instance Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11RWResource>> FInInst;

        //[Input("Enabled")]
        //IDiffSpread<bool> FInEnabled;


        [Output("Geometry Out")]
        protected ISpread<DX11Resource<DX11IndexedGeometry>> FOutGeom;

        bool invalidate = false;

        public void Evaluate(int SpreadMax)
        {
            invalidate = false;

            if (this.FInGeom.IsConnected)
            {
                this.FOutGeom.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++) 
                {
                    if (this.FOutGeom[i] == null)
                    {
                        this.FOutGeom[i] = new DX11Resource<DX11IndexedGeometry>();
                    }
                }

                invalidate = this.FInGeom.IsChanged || this.FInCnt.IsChanged;

            }
            else
            {
                this.FOutGeom.SliceCount = 0;
            }
        }

        public void Update(DX11RenderContext context)
        {
            Device device = context.Device;
            DeviceContext ctx = context.CurrentDeviceContext;

            for (int i = 0; i < this.FOutGeom.SliceCount; i++)
            {
                DX11IndexedGeometry geom;
                if (this.invalidate || (!this.FOutGeom[i].Contains(context)))
                {
                    if (this.FOutGeom[i].Contains(context))
                    {
                        var g = this.FOutGeom[i][context];
                        DX11IndexedIndirectDrawer d = (DX11IndexedIndirectDrawer)g.Drawer;

                        if (d != null)
                        {
                            d.IndirectArgs.Dispose();
                        }
                    }

                    geom = (DX11IndexedGeometry)this.FInGeom[i][context].ShallowCopy();

                    DX11IndexedIndirectDrawer ind = new DX11IndexedIndirectDrawer();
                    geom.AssignDrawer(ind);

                    ind.Update(context, this.FInCnt[i]);

                    this.invalidate = false;
                }
                else
                {
                    geom = this.FOutGeom[i][context];
                }

                DX11IndexedIndirectDrawer drawer = (DX11IndexedIndirectDrawer)geom.Drawer;

                if (this.FInIdx.IsConnected)
                {
                    drawer.IndirectArgs.CopyIndicesCount(ctx, this.FInIdx[i][context].UAV);
                }

                if (this.FInInst.IsConnected)
                {
                    drawer.IndirectArgs.CopyInstanceCount(ctx, this.FInInst[i][context].UAV);
                }

                this.FOutGeom[i][context] = geom;
            }

        }

        public void Destroy(DX11RenderContext OnDevice, bool force)
        {
            for (int i = 0; i < this.FOutGeom.SliceCount; i++ )
            {

                try
                {
                    var geom = this.FOutGeom[i][OnDevice];
                    DX11IndexedIndirectDrawer drawer = (DX11IndexedIndirectDrawer)geom.Drawer;
                    drawer.IndirectArgs.Dispose();
                }
                catch
                { }

                this.FOutGeom[i].Remove(OnDevice);
            }
                
        }
    }
}
