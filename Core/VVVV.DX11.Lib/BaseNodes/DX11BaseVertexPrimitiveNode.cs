using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using SlimDX.Direct3D11;

using FeralTic.DX11.Resources;
using FeralTic.DX11;
namespace VVVV.DX11.Nodes
{
    public abstract class DX11BaseVertexPrimitiveNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {

        [Output("Geometry Out", Order = 5)]
        protected Pin<DX11Resource<DX11VertexGeometry>> FOutput;

        protected bool FInvalidate;
        protected int oldSpreadMax = 0;

        protected abstract DX11VertexGeometry GetGeom(DX11RenderContext context, int slice);
        protected abstract bool Invalidate();
        protected virtual int GetSpreadMax(int spreadmax) { return spreadmax; }

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            int spm = this.GetSpreadMax(SpreadMax);

            if (spm != oldSpreadMax || this.Invalidate())
            {
                this.FInvalidate = true;

                //Dispose old
                this.FOutput.SafeDisposeAll();

                this.FOutput.SliceCount = spm;
                this.oldSpreadMax = spm;

                for (int i = 0; i < spm; i++)
                {
                    this.FOutput[i] = new DX11Resource<DX11VertexGeometry>();
                }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInvalidate || !this.FOutput[0].Contains(context))
            {
                for (int i = 0; i < oldSpreadMax; i++)
                {
                    DX11VertexGeometry geom = this.GetGeom(context, i);
                    this.FOutput[i][context] = geom;
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutput.SafeDisposeAll(context);
        }

        public void Dispose()
        {
            this.FOutput.SafeDisposeAll();
        }
    }
}
