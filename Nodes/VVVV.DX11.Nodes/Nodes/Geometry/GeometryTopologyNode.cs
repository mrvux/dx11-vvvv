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
    [PluginInfo(Name = "Topology", Category = "DX11.Geometry", Version = "", Author = "vux")]
    public class GeometryTopologyNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In",CheckIfChanged=true)]
        protected Pin<DX11Resource<IDX11Geometry>> FInGeom;

        [Input("Topology")]
        protected IDiffSpread<PrimitiveTopology> FInTopology;

        [Input("Enabled")]
        protected IDiffSpread<bool> FInEnabled;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<IDX11Geometry>> FOutGeom;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInGeom.IsConnected)
            {
                this.FOutGeom.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++) { if (this.FOutGeom[i] == null) { this.FOutGeom[i] = new DX11Resource<IDX11Geometry>(); } }

                this.FOutGeom.Stream.IsChanged = SpreadMax > 0 && this.FInEnabled.SliceCount > 0 ? this.FInEnabled[0] : false;
            }
            else
            {
                this.FOutGeom.SliceCount = 0;
            }
        }

        public void Update(DX11RenderContext context)
        {
            Device device = context.Device;

            for (int i = 0; i < this.FOutGeom.SliceCount; i++)
            {
                if (this.FInEnabled[i] && this.FInTopology[i] != PrimitiveTopology.Undefined && this.FInGeom[i].Contains(context))
                {

                    IDX11Geometry geom = this.FInGeom[i][context].ShallowCopy();
                    geom.Topology = this.FInTopology[i];
                    this.FOutGeom[i][context] = geom;
                }
                else
                {
                    this.FOutGeom[i][context] = this.FInGeom[i][context];
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }
    }
}
