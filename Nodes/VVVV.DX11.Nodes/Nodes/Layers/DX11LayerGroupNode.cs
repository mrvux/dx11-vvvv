using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Queries;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name="Group",Category="DX11.Layer",Author="vux")]
    public class DX11LayerGroupNode : IPluginEvaluate, IDX11LayerProvider, IDX11Queryable, IPartImportsSatisfiedNotification, IDX11UpdateBlocker
    {
        [Config("Input Count", DefaultValue = 2, MinValue = 2)]
        protected IDiffSpread<int> FInputCount;

        [Input("Render State", IsSingle=true)]
        protected Pin<DX11RenderState> FInState;

        [Input("Custom Semantics", Order = 5000, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<IDX11RenderSemantic> FInSemantics;

        [Input("Resource Semantics", Order = 5001, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<DX11Resource<IDX11RenderSemantic>> FInResSemantics;

        [Input("Validators", Order = 5001, Visibility = PinVisibility.OnlyInspector)]
        protected Pin<IDX11ObjectValidator> FInVal;

        [Input("Enabled",DefaultValue=1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        private List<IIOContainer<Pin<DX11Resource<DX11Layer>>>> FLayers = new List<IIOContainer<Pin<DX11Resource<DX11Layer>>>>();

        private IPluginHost FHost;
        private IIOFactory FIOFactory;

        private int spmax;


        [ImportingConstructor()]
        public DX11LayerGroupNode(IPluginHost host,IIOFactory iofactory)
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
                if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }

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
                        attr.IsSingle = true;
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

        public void Update(IPluginIO pin, DX11RenderContext context)
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

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            this.FOutLayer[0].Dispose(context);
        }

        public void Render(IPluginIO pin, DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.spmax > 0)
            {
                if (this.FEnabled[0])
                {
                    bool popstate = false;

                    if (this.FInState.PluginIO.IsConnected)
                    {
                        context.RenderStateStack.Push(this.FInState[0]);
                        popstate = true;
                    }

                    List<IDX11RenderSemantic> semantics = new List<IDX11RenderSemantic>();
                    if (this.FInSemantics.PluginIO.IsConnected)
                    {
                        semantics.AddRange(this.FInSemantics);
                        settings.CustomSemantics.AddRange(semantics);
                    }


                    List<DX11Resource<IDX11RenderSemantic>> ressemantics = new List<DX11Resource<IDX11RenderSemantic>>();
                    if (this.FInResSemantics.PluginIO.IsConnected)
                    {
                        ressemantics.AddRange(this.FInResSemantics);
                        settings.ResourceSemantics.AddRange(ressemantics);
                    }


                    List<IDX11ObjectValidator> valids = new List<IDX11ObjectValidator>();
                    if (this.FInVal.PluginIO.IsConnected)
                    {
                        for (int i = 0; i < this.FInVal.SliceCount; i++)
                        {
                            if (this.FInVal[i].Enabled)
                            {
                                IDX11ObjectValidator v = this.FInVal[i];
                                //v.Reset();
                                v.SetGlobalSettings(settings);

                                valids.Add(v);
                                settings.ObjectValidators.Add(v);
                            }
                        }
                    }



                    if (this.BeginQuery != null)
                    {
                        this.BeginQuery(context);
                    }
                    
                    foreach (IIOContainer<Pin<DX11Resource<DX11Layer>>> dxpin in this.FLayers)
                    {
                        if (dxpin.IOObject.PluginIO.IsConnected)
                        {
                            try
                            {
                                dxpin.IOObject[0][context].Render(dxpin.IOObject.PluginIO, context, settings);
                            }
                            catch
                            { }
                        }
                    }

                    if (this.EndQuery != null)
                    {
                        this.EndQuery(context);
                    }

                    foreach (IDX11RenderSemantic semantic in semantics)
                    {
                        settings.CustomSemantics.Remove(semantic);
                    }

                    foreach (DX11Resource<IDX11RenderSemantic> rs in ressemantics)
                    {
                        settings.ResourceSemantics.Remove(rs);
                    }

                    foreach (IDX11ObjectValidator v in valids)
                    {
                        settings.ObjectValidators.Remove(v);
                    }

                    if (popstate) { context.RenderStateStack.Pop(); }
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

        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        public bool Enabled
        {
            get 
            {
                if (this.spmax > 0)
                {
                    return this.FEnabled[0];
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
