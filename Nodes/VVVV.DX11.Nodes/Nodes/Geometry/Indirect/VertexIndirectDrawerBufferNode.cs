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
    [PluginInfo(Name = "VertexIndirect", Category = "DX11.Drawer", Version = "Buffer", Author = "vux")]
    public class VertexIndirectDrawerBufferNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In", CheckIfChanged = true)]
        protected Pin<DX11Resource<DX11VertexGeometry>> FInGeom;

        [Input("Default Instance Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInCnt;

        [Input("Vertex Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11Buffer>> FInV;

        [Input("Vertex Resource Offset", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FInVtxOffset;

        [Input("Instance Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11Buffer>> FInI;

        [Input("Instance Resource Offset", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FInInstOffset;

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
                DX11VertexGeometry geom;
                if (this.invalidate || (!this.FOutGeom[i].Contains(context)))
                {
                    geom = (DX11VertexGeometry)this.FInGeom[i][context].ShallowCopy();

                    DX11VertexIndirectDrawer ind = new DX11VertexIndirectDrawer();
                    geom.AssignDrawer(ind);

                    ind.Update(context, this.FInCnt[i]);

                    this.invalidate = false;
                }
                else
                {
                    geom = this.FOutGeom[i][context];
                }

                DX11VertexIndirectDrawer drawer = (DX11VertexIndirectDrawer)geom.Drawer;
                var argBuffer = drawer.IndirectArgs.Buffer;

                if (this.FInI.IsConnected)
                {
                    int instOffset = this.FInInstOffset[i];
                    ResourceRegion region = new ResourceRegion(instOffset, 0, 0, instOffset + 4, 1, 1);
                    context.CurrentDeviceContext.CopySubresourceRegion(this.FInI[i][context].Buffer, 0, region, argBuffer, 0, 4, 0, 0);
                }

                if (this.FInV.IsConnected)
                {
                    int vOffset = this.FInVtxOffset[i];
                    ResourceRegion region = new ResourceRegion(vOffset, 0, 0, vOffset + 4, 1, 1);
                    context.CurrentDeviceContext.CopySubresourceRegion(this.FInV[i][context].Buffer, 0, region, argBuffer, 0, 0, 0, 0);
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
