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
    [PluginInfo(Name = "BoundingBox", Category = "DX11.Geometry", Version = "Set", Author = "vux")]
    public class SetBoundingBoxGeometryNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Geometry In", CheckIfChanged = true)]
        protected Pin<DX11Resource<IDX11Geometry>> FInGeom;

        [Input("Minimum",DefaultValue=-1)]
        protected ISpread<Vector3> FMin;

        [Input("Maximum",DefaultValue=1)]
        protected ISpread<Vector3> FMax;

        [Input("Enabled",DefaultValue=1)]
        protected ISpread<bool> FEnabled;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<IDX11Geometry>> FOutGeom;

        private int spmax;

        public void Evaluate(int SpreadMax)
        {
            this.FOutGeom.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutGeom[i] == null) { this.FOutGeom[i] = new DX11Resource<IDX11Geometry>(); }
            }

            this.spmax = SpreadMax;
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInGeom.IsConnected)
            {
                for (int i = 0; i < spmax; i++)
                {
                    if (this.FEnabled[i])
                    {
                        IDX11Geometry g = this.FInGeom[i][context].ShallowCopy();
                        BoundingBox b = new BoundingBox(this.FMin[i], this.FMax[i]);
                        g.HasBoundingBox = true;
                        g.BoundingBox = b;
                        this.FOutGeom[i][context] = g;
                    }
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            
        }
    }
}
