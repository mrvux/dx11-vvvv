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
    [PluginInfo(Name = "DispatchIndirect", Category = "DX11.Drawer", Version = "1D", Author = "vux")]
    public class DispatchIndirectDrawerNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Warp Size", DefaultValue = 1, IsSingle=true)]
        protected IDiffSpread<int> FInWarpX;

        [Input("Argument Buffer",IsSingle = true)]
        protected Pin<DX11Resource<IDX11RWResource>> FInArgBuffer;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<DX11NullGeometry>> FOutGeom;

        [Output("Counter Buffer")]
        protected ISpread<DX11Resource<IDX11StructuredBuffer>> FOutCounter;

        private DX11ShaderInstance generateShader;
        private DispatchIndirectBuffer dispatchBuffer;
        private DX11RawBuffer countBuffer;

        private DX11NullIndirectDispatcher indirectDispatch;

        public void Evaluate(int SpreadMax)
        {
            if (FInArgBuffer.IsConnected)
            {
                if (this.FOutGeom.SliceCount == 0)
                {
                    this.FOutGeom.SliceCount = 1;
                    this.FOutCounter.SliceCount = 1;
                }

                if (this.FOutGeom[0] == null)
                {
                    this.FOutGeom[0] = new DX11Resource<DX11NullGeometry>();
                    this.FOutCounter[0] = new DX11Resource<IDX11StructuredBuffer>();
                }
            }
            else
            {
                this.FOutGeom.SliceCount = 0;
                this.FOutCounter.SliceCount = 0;
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FOutGeom.SliceCount == 0) { return; }

            if (this.generateShader == null)
            {
                this.generateShader = ShaderUtils.GetShader(context, "GenerateDispatch1D");
                this.dispatchBuffer = new DispatchIndirectBuffer(context);
                this.countBuffer = new DX11RawBuffer(context.Device, 16);
            }

            if (!this.FOutGeom[0].Contains(context))
            {
                this.indirectDispatch = new DX11NullIndirectDispatcher();
                this.indirectDispatch.IndirectArgs = this.dispatchBuffer;

                DX11NullGeometry nullgeom = new DX11NullGeometry(context);
                nullgeom.AssignDrawer(this.indirectDispatch);

                this.FOutGeom[0][context] = nullgeom;
                this.FOutCounter[0][context] = this.dispatchBuffer.RWBuffer;
            }

            var countuav = this.FInArgBuffer[0][context];

            context.CurrentDeviceContext.CopyStructureCount(countuav.UAV, this.countBuffer.Buffer, 0);

            this.generateShader.SetBySemantic("WARPSIZE", this.FInWarpX[0]);
            this.generateShader.SetBySemantic("COUNTERBUFFER", this.countBuffer.SRV);
            this.generateShader.SetBySemantic("RWDISPATCHBUFFER", this.dispatchBuffer.UAV);

            this.generateShader.ApplyPass(0);

            context.CurrentDeviceContext.Dispatch(1, 1, 1);
            this.generateShader.CleanUp();

            this.dispatchBuffer.UpdateBuffer();
        }

        public void Destroy(DX11RenderContext OnDevice, bool force)
        {
        }

        public void Dispose()
        {
            if (this.generateShader != null)
            {
                this.generateShader.Dispose();
                this.dispatchBuffer.Dispose();
                this.countBuffer.Dispose();
            }
        }
    }
}
