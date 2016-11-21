using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using SlimDX.Direct3D11;


namespace VVVV.DX11.Nodes.Geometry.Primitives
{
    [PluginInfo(Name = "Dispatcher", Category = "DX11.Drawer", Version = "", Author = "vux")]
    public class DX11DispatcherNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Thread X", DefaultValue = 1, MinValue=0)]
        protected IDiffSpread<int> FInTX;

        [Input("Thread Y", DefaultValue = 1, MinValue = 0)]
        protected IDiffSpread<int> FInTY;

        [Input("Thread Z", DefaultValue = 1, MinValue = 0)]
        protected IDiffSpread<int> FInTZ;

        [Output("Geometry Out", Order = 5)]
        protected Pin<DX11Resource<IDX11Geometry>> FOutput;

        private bool FInvalidate;

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            this.FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutput[i] == null)
                {
                    this.FOutput[i] = new DX11Resource<IDX11Geometry>();
                }
            }

            if (this.FInTX.IsChanged || this.FInTY.IsChanged || this.FInTZ.IsChanged)
            {
                this.FInvalidate = true;
            }
        }

        public void Update(DX11RenderContext context)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                if (this.FInvalidate || !this.FOutput[i].Contains(context))
                {
                    if (this.FOutput[i].Contains(context)) { this.FOutput[i].Dispose(context); }

                    DX11NullDispatcher disp = new DX11NullDispatcher();
                    disp.X = Math.Max(this.FInTX[i], 0);
                    disp.Y = Math.Max(this.FInTY[i], 0);
                    disp.Z = Math.Max(this.FInTZ[i], 0);

                    DX11NullGeometry geom = new DX11NullGeometry(context, disp);

                    geom.Topology = PrimitiveTopology.Undefined;
                    geom.InputLayout = new InputElement[i];
                    geom.HasBoundingBox = false;

                    this.FOutput[i][context] = geom;
                }
            }

        }

        public void Destroy(DX11RenderContext context, bool force)
        {
        }
    }
}
