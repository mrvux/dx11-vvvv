using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using FeralTic.Resources.Geometry;
using SlimDX.Direct3D11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name="AttachDispatcher",Category="DX11.Layer",Version="", Author="vux")]
    public class DX11LayerDispatcherNode : IPluginEvaluate, IDX11LayerHost
    {
        [Input("Bind Offset", IsSingle = true)]
        protected Pin<int> FInOffset;

        [Input("Layer In", AutoValidate = false)]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Enabled",DefaultValue=1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        private DX11NullGeometry geometry;
        private DX11BufferDispatcher dispatcher;


        public void Evaluate(int SpreadMax)
        {
            if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }
        }


        #region IDX11ResourceProvider Members

        public void Update(DX11RenderContext context)
        {
            if (this.dispatcher == null)
            {
                this.dispatcher = new DX11BufferDispatcher();
                this.geometry = new DX11NullGeometry(context, this.dispatcher);
            }

            if (!this.FOutLayer[0].Contains(context))
            {
                this.FOutLayer[0][context] = new DX11Layer();
                this.FOutLayer[0][context].Render = this.Render;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutLayer.SafeDisposeAll(context);
        }

        public void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            IDX11Geometry g = settings.Geometry;
            if (this.FEnabled[0])
            {
                if (this.FLayerIn.IsConnected)
                {
                    IDX11Geometry geom = settings.Geometry;

                    settings.Geometry = null;
                    if (settings.BackBuffer is IDX11Buffer)
                    {
                        IDX11Buffer buffer = settings.BackBuffer as IDX11Buffer;
                        if (buffer.Buffer.Description.OptionFlags.HasFlag(ResourceOptionFlags.DrawIndirect))
                        {
                            this.dispatcher.DispatchBuffer = buffer.Buffer;
                            this.dispatcher.Offet = this.FInOffset[0];
                            settings.Geometry = this.geometry;
                        }
                    }

                    for (int i = 0; i < this.FLayerIn.SliceCount; i++)
                    {
                        this.FLayerIn[i][context].Render(context, settings);
                    }

                    settings.Geometry = geom;
                }
            }
            else
            {
                if (this.FLayerIn.IsConnected)
                {
                    for (int i = 0; i < this.FLayerIn.SliceCount; i++)
                    {
                        this.FLayerIn[i][context].Render(context, settings);
                    }
                }
            }
            settings.Geometry = g;
        }

        #endregion


    }
}
