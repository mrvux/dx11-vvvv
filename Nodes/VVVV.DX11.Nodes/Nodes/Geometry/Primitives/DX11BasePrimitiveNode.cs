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
    public abstract class DX11BasePrimitiveNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Keep In Memory", IsSingle =true, Order = 5000, Visibility =PinVisibility.OnlyInspector)]
        protected ISpread<bool> FKeepInMemory;

        [Output("Geometry Out", Order = 5)]
        protected Pin<DX11Resource<DX11IndexedGeometry>> FOutput;

        protected bool FInvalidate;
        protected int oldSpreadMax = 0;

        protected abstract DX11IndexedGeometry GetGeom(DX11RenderContext context, int slice);
        protected abstract bool Invalidate();

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (SpreadMax != oldSpreadMax || this.Invalidate())
            {
                this.FInvalidate = true;

                //Dispose old
                this.FOutput.SafeDisposeAll();

                this.FOutput.SliceCount = SpreadMax;
                this.oldSpreadMax = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FOutput[i] = new DX11Resource<DX11IndexedGeometry>();
                }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInvalidate || !this.FOutput[0].Contains(context))
            {
                for (int i = 0; i < oldSpreadMax; i++)
                {
                    DX11IndexedGeometry geom = this.GetGeom(context, i);
                    this.FOutput[i][context] = geom;   
                }
                this.FInvalidate = false;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (FKeepInMemory[0] == false || force)
            {
                this.FOutput.SafeDisposeAll(context);
            }
        }

        public void Dispose()
        {
            this.FOutput.SafeDisposeAll();
        }
    }
}
