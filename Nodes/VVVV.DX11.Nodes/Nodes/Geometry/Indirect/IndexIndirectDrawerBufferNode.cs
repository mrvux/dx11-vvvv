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
    [PluginInfo(Name = "IndexIndirect", Category = "DX11.Drawer", Version = "Buffer", Author = "vux")]
    public class IndexIndirectDrawerBufferNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In", CheckIfChanged = true)]
        protected Pin<DX11Resource<DX11IndexedGeometry>> FInGeom;

        [Input("Default Instance Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInCnt;

        [Input("Index Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11Buffer>> FInIdx;

        [Input("Index Resource Offset", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FInIdxOffset;

        [Input("Instance Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11Buffer>> FInInst;

        [Input("Instance Resource Offset", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FInInstOffset;

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

                var argBuffer = drawer.IndirectArgs.Buffer;
                if (this.FInIdx.IsConnected)
                {
                    int idxOffset = this.FInIdxOffset[i];

                    ResourceRegion region = new ResourceRegion(idxOffset, 0, 0, idxOffset+ 4, 1, 1);
                    context.CurrentDeviceContext.CopySubresourceRegion(this.FInIdx[i][context].Buffer, 0, region, argBuffer, 0, 0, 0, 0);
                }

                if (this.FInInst.IsConnected)
                {
                    int instOffset = this.FInInstOffset[i];
                    ResourceRegion region = new ResourceRegion(instOffset, 0, 0,instOffset + 4, 1, 1);
                    context.CurrentDeviceContext.CopySubresourceRegion(this.FInInst[i][context].Buffer, 0, region, argBuffer, 0, 4, 0, 0);
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
