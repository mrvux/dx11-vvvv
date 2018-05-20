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
    [PluginInfo(Name = "BoundingBox", Category = "DX11.Geometry", Version = "Get", Author = "vux")]
    public class BoundingBoxGeometryNode : IPluginEvaluate, IDX11ResourceDataRetriever
    {
        [Input("Geometry In", CheckIfChanged = true)]
        protected Pin<DX11Resource<IDX11Geometry>> FInGeom1;

        [Output("Minimum")]
        protected ISpread<Vector3> FOutMin;

        [Output("Maximum")]
        protected ISpread<Vector3> FOutMax;

        [Output("Is Valid")]
        protected ISpread<bool> FOutValid;

        [Import()]
        protected IPluginHost FHost;

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInGeom1.IsConnected)
            {
                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null) { this.SetNull(); return; }

                //Do NOT cache this, assignment done by the host
                Device device = this.AssignedContext.Device;

                this.FOutMin.SliceCount = this.FInGeom1.SliceCount;
                this.FOutMax.SliceCount = this.FInGeom1.SliceCount;
                this.FOutValid.SliceCount = this.FInGeom1.SliceCount;

                for (int i = 0; i < this.FInGeom1.SliceCount; i++)
                {
                    if (this.FInGeom1[i].Contains(this.AssignedContext))
                    {
                        if (this.FInGeom1[i][this.AssignedContext].HasBoundingBox)
                        {
                            this.FOutMin[i] = this.FInGeom1[i][this.AssignedContext].BoundingBox.Minimum;
                            this.FOutMax[i] = this.FInGeom1[i][this.AssignedContext].BoundingBox.Maximum;
                            this.FOutValid[i] = true;
                        }
                        else
                        {
                            this.FOutMin[i] = Vector3.Zero;
                            this.FOutMax[i] = Vector3.Zero;
                            this.FOutValid[i] = false;
                        }
                    }
                    else
                    {
                        this.FOutMin[i] = Vector3.Zero;
                        this.FOutMax[i] = Vector3.Zero;
                        this.FOutValid[i] = false;
                    }
                }
            }
            else
            {
                this.SetNull();
            }
        }

        private void SetNull()
        {
            this.FOutMin.SliceCount = 0;
            this.FOutMax.SliceCount = 0;
            this.FOutValid.SliceCount = 0;
        }
    }



}
