using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Queries;
using VVVV.Utils.VMath;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name="ViewportRouter",Category="DX11.Layer",Author="vux")]
    public class DX11LayerViewportRouterNode : IPluginEvaluate, IDX11LayerHost, IPartImportsSatisfiedNotification
    {
        [Config("Input Count", DefaultValue = 2, MinValue = 2)]
        protected IDiffSpread<int> FInputCount;

        [Input("Offset",IsSingle =true, DefaultValue = 0, Order = -1)]
        protected ISpread<int> FOffset;

        [Input("Enabled",DefaultValue=1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        private List<IIOContainer<Pin<DX11Resource<DX11Layer>>>> FLayers = new List<IIOContainer<Pin<DX11Resource<DX11Layer>>>>();

        private IPluginHost FHost;
        private IIOFactory FIOFactory;

        private int spmax;


        [ImportingConstructor()]
        public DX11LayerViewportRouterNode(IPluginHost host,IIOFactory iofactory)
        {
            this.FHost = host;
            this.FIOFactory = iofactory;
        }

        public void Evaluate(int SpreadMax)
        {
            this.spmax = SpreadMax;
            if (SpreadMax > 0)
            {
                if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }

                if (this.FEnabled[0])
                {
                    foreach (IIOContainer<Pin<DX11Resource<DX11Layer>>> lin in this.FLayers)
                    {
                        lin.IOObject.Sync();
                    }
                }
            }
        }

        private void SetInputs()
        {
            if (this.FInputCount[0] != FLayers.Count)
            {
                if (this.FInputCount[0] > FLayers.Count)
                {
                    while (this.FInputCount[0] > FLayers.Count)
                    {
                        InputAttribute attr = new InputAttribute("Layer " + Convert.ToString(this.FLayers.Count + 1));
                        attr.IsSingle = false;
                        attr.CheckIfChanged = true;
                        attr.AutoValidate = false;
                        //Create new layer Pin
                        IIOContainer<Pin<DX11Resource<DX11Layer>>> newlayer = this.FIOFactory.CreateIOContainer<Pin<DX11Resource<DX11Layer>>>(attr);
                        newlayer.IOObject.SliceCount = 1;
                        this.FLayers.Add(newlayer);
                        newlayer.IOObject[0] = new DX11Resource<DX11Layer>();
                    }
                }
                else
                {
                    while (this.FInputCount[0] < FLayers.Count)
                    {
                        this.FLayers[this.FLayers.Count - 1].Dispose();
                        this.FLayers.RemoveAt(this.FLayers.Count - 1);
                    }
                }
            }
        }

        #region IDX11ResourceProvider Members

        public void Update(DX11RenderContext context)
        {
            if (this.spmax > 0)
            {
                if (!this.FOutLayer[0].Contains(context))
                {
                    this.FOutLayer[0][context] = new DX11Layer();
                    this.FOutLayer[0][context].Render = this.Render;
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutLayer.SafeDisposeAll(context);
        }

        public void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.spmax > 0)
            {
                if (this.FEnabled[0])
                {
                    int viewportIndex = VMath.Zmod(settings.ViewportIndex + FOffset[0], this.FLayers.Count);
                    var dxpin = this.FLayers[viewportIndex];
                    if (dxpin.IOObject.IsConnected)
                    {
                        dxpin.IOObject.RenderAll(context, settings);
                    }

                }
            }
        }

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.FInputCount.Changed += new SpreadChangedEventHander<int>(FInputCount_Changed);
            this.SetInputs();
        }

        void FInputCount_Changed(IDiffSpread<int> spread)
        {
            this.SetInputs();
        }

        #endregion

    }
}
