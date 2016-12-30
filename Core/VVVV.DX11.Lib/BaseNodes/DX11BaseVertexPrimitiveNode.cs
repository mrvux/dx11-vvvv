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
    public abstract class DX11BaseVertexPrimitiveNode : IPluginEvaluate, IDX11ResourceProvider, IDisposable
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
                for (int i = 0; i < oldSpreadMax; i++)
                {
                    this.FOutput[i].Dispose();
                }

                this.FOutput.SliceCount = spm;
                this.oldSpreadMax = spm;

                for (int i = 0; i < spm; i++)
                {
                    this.FOutput[i] = new DX11Resource<DX11VertexGeometry>();
                }
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (this.FInvalidate || !this.FOutput[0].Data.ContainsKey(context))
            {
                for (int i = 0; i < oldSpreadMax; i++)
                {
                    DX11VertexGeometry geom = this.GetGeom(context, i);
                    this.FOutput[i][context] = geom;
                }
            }
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                if (this.FOutput[i] != null)
                {
                    this.FOutput[i].Dispose(context);
                }
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                if (this.FOutput[i] != null)
                {
                    this.FOutput[i].Dispose();
                }
            }
        }
    }
}
