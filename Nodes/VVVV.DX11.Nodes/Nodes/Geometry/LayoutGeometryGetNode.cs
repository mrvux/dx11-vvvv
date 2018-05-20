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
    [PluginInfo(Name = "InputLayout", Category = "DX11.Geometry", Version = "Get", Author = "vux")]
    public class InputLayoutGeometryGetNode : IPluginEvaluate, IDX11ResourceDataRetriever
    {
        [Input("Geometry In", CheckIfChanged = true)]
        protected Pin<DX11Resource<IDX11Geometry>> FInGeom1;

        [Output("Layout")]
        protected ISpread<ISpread<InputElement>> FOutLayout;

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

                this.FOutLayout.SliceCount = SpreadMax;
                this.FOutValid.SliceCount = SpreadMax;

                for (int i = 0; i < this.FInGeom1.SliceCount; i++)
                {
                    if (this.FInGeom1[i].Contains(this.AssignedContext))
                    {
                        IDX11Geometry geom = this.FInGeom1[i][this.AssignedContext];
                        this.FOutLayout[i].SliceCount = geom.InputLayout.Length;

                        for (int j = 0; j < geom.InputLayout.Length; j++)
                        {
                            this.FOutLayout[i][j] = geom.InputLayout[j];
                        }

                        this.FOutValid[i] = true;

                    }
                    else
                    {
                        this.FOutLayout[i].SliceCount = 0;
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
            this.FOutLayout.SliceCount = 0;
            this.FOutValid.SliceCount = 0;
        }
    }



}
