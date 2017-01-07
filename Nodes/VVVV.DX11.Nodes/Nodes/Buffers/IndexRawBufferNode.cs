using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "RawIndexBuffer", Category = "DX11", Version = "Geometry")]
    public class IndexRawBufferNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In", IsSingle = true)]
        protected Pin<DX11Resource<IDX11Geometry>> FIn;

        [Output("Buffer", IsSingle = true)]
        protected ISpread<DX11Resource<IDX11ReadableResource>> FOutBuffer;

        [Output("Valid", IsSingle = true)]
        protected ISpread<bool> FOutValid;

        public void Evaluate(int SpreadMax)
        {
            if (this.FIn.IsConnected)
            {
                this.FOutBuffer.SliceCount = SpreadMax;
                for (int i = 0; i < this.FIn.SliceCount; i++)
                {
                    if (this.FOutBuffer[i] == null) { this.FOutBuffer[i] = new DX11Resource<IDX11ReadableResource>(); }
                }
            }
            else
            {
                this.FOutBuffer.SliceCount = 0;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }

        public void Update(DX11RenderContext context)
        {
            for (int i = 0; i < this.FIn.SliceCount; i++)
            {
                IDX11Geometry geom = this.FIn[i][context];

                if (geom is DX11IndexedGeometry)
                {
                    DX11IndexedGeometry g = (DX11IndexedGeometry)geom;
                    this.FOutBuffer[i][context] = g.IndexBuffer;
                    this.FOutValid[i] = true;
                }
                else if (geom is DX11IndexOnlyGeometry)
                {
                    DX11IndexOnlyGeometry g = (DX11IndexOnlyGeometry)geom;
                    this.FOutBuffer[i][context] = g.IndexBuffer;
                    this.FOutValid[i] = true;
                }
                else
                {
                    this.FOutBuffer[i][context] = null;
                    this.FOutValid[i] = false;
                }

                
            }
        }


    }
}
