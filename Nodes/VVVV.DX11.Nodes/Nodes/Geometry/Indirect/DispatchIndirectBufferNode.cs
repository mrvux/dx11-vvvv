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
using FeralTic.Resources.Geometry;

namespace VVVV.DX11.Nodes.Geometry
{
    [PluginInfo(Name = "DispatchIndirect", Category = "DX11.Drawer", Version = "Buffer", Author = "vux")]
    public class DispatchIndirectDrawerBufferNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Argument Buffer",IsSingle = true)]
        protected Pin<DX11Resource<IDX11Buffer>> FInArgBuffer;

        [Input("Argument Resource Offset", DefaultValue = 0)]
        protected ISpread<int> FInArgOffset;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<DX11NullGeometry>> FOutGeom;


        private DispatchIndirectBuffer dispatchBuffer;
        private DX11NullIndirectDispatcher indirectDispatch;

        public void Evaluate(int SpreadMax)
        {
            if (FInArgBuffer.IsConnected)
            {
                if (this.FOutGeom.SliceCount == 0)
                {
                    this.FOutGeom.SliceCount = 1;
                }

                if (this.FOutGeom[0] == null)
                {
                    this.FOutGeom[0] = new DX11Resource<DX11NullGeometry>();
                }
            }
            else
            {
                this.FOutGeom.SliceCount = 0;
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FOutGeom.SliceCount == 0) { return; }

            if (this.dispatchBuffer == null)
            {
                this.dispatchBuffer = new DispatchIndirectBuffer(context);
            }

            if (!this.FOutGeom[0].Contains(context))
            {
                this.indirectDispatch = new DX11NullIndirectDispatcher();
                this.indirectDispatch.IndirectArgs = this.dispatchBuffer;

                DX11NullGeometry nullgeom = new DX11NullGeometry(context);
                nullgeom.AssignDrawer(this.indirectDispatch);

                this.FOutGeom[0][context] = nullgeom;
            }

            var countuav = this.FInArgBuffer[0][context];

            var argBuffer = this.dispatchBuffer.Buffer;

            int argOffset = this.FInArgOffset[0];
            ResourceRegion region = new ResourceRegion(argOffset, 0, 0, argOffset + 12, 1, 1); //Packed xyz value here
            context.CurrentDeviceContext.CopySubresourceRegion(this.FInArgBuffer[0][context].Buffer, 0, region, argBuffer, 0, 0, 0, 0);
        }

        public void Destroy(DX11RenderContext OnDevice, bool force)
        {
        }

        public void Dispose()
        {
            if (this.dispatchBuffer != null)
            {
                this.dispatchBuffer.Dispose();
            }
        }
    }
}
