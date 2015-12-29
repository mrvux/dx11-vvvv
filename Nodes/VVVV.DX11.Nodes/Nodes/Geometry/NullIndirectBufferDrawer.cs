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
    [PluginInfo(Name = "NullIndirect", Category = "DX11.Drawer", Version = "Buffer", Author = "vux")]
    public class NullIndirectBufferDrawerNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Default Vertex Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInVCnt;

        [Input("Default Instance Count", DefaultValue = 1)]
        protected IDiffSpread<int> FInICnt;

        [Input("Vertex Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11Buffer>> FInV;

        [Input("Vertex Resource Offset", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FInVtxOffset;

        [Input("Instance Arg Buffer", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11Buffer>> FInI;

        [Input("Instance Resource Offset", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FInInstOffset;


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

                    var argBuffer = drawer.IndirectArgs.Buffer;

                    if (this.FInI.PluginIO.IsConnected)
                    {
                        int instOffset = this.FInInstOffset[i];
                        ResourceRegion region = new ResourceRegion(instOffset, 0, 0, instOffset + 4, 1, 1);
                        context.CurrentDeviceContext.CopySubresourceRegion(this.FInI[i][context].Buffer, 0, region, argBuffer, 0, 4, 0, 0);
                    }

                    if (this.FInV.PluginIO.IsConnected)
                    {
                        int vOffset = this.FInVtxOffset[i];
                        ResourceRegion region = new ResourceRegion(vOffset, 0, 0, vOffset + 4, 1, 1);
                        context.CurrentDeviceContext.CopySubresourceRegion(this.FInV[i][context].Buffer, 0, region, argBuffer, 0, 0, 0, 0);
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
