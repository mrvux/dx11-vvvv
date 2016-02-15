using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Switch", Category = "DX11.IndexedGeometry", Version = "2d", Author = "vux")]
    public class SwitchGeometryNode : IPluginEvaluate, IDX11ResourceProvider, IPartImportsSatisfiedNotification
    {
        [Input("Switch", Order = -5)]
        protected ISpread<int> FInSwitch;

        [Config("Input Count", DefaultValue = 2, MinValue = 2)]
        protected IDiffSpread<int> FInputCount;

        [Output("Output")]
        protected ISpread<DX11Resource<DX11IndexedGeometry>> FOutput;

        private List<IIOContainer<Pin<DX11Resource<DX11IndexedGeometry>>>> FInputs = new List<IIOContainer<Pin<DX11Resource<DX11IndexedGeometry>>>>();

        [Import()]
        protected IPluginHost FHost;

        [Import()]
        protected IIOFactory FIOFactory;

        public void Evaluate(int SpreadMax)
        {
            //
            this.FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                int idx = VMath.Zmod(FInSwitch[i], FInputs.Count);

                

                var pin = FInputs[idx].IOObject;

                this.FOutput[i] = pin.PluginIO.IsConnected ? pin[i] : new DX11Resource<DX11IndexedGeometry>();
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {

        }

        #region Set Inputs
        private void SetInputs()
        {

            if (this.FInputCount[0] != FInputs.Count)
            {
                if (this.FInputCount[0] > FInputs.Count)
                {
                    while (this.FInputCount[0] > FInputs.Count)
                    {
                        InputAttribute attr = new InputAttribute("Input " + Convert.ToString(this.FInputs.Count + 1));
                        //attr.IsSingle = true;
                        attr.CheckIfChanged = true;
                        //Create new layer Pin
                        IIOContainer<Pin<DX11Resource<DX11IndexedGeometry>>> newlayer = this.FIOFactory.CreateIOContainer<Pin<DX11Resource<DX11IndexedGeometry>>>(attr);
                        newlayer.IOObject.SliceCount = 1;
                        this.FInputs.Add(newlayer);
                    }
                }
                else
                {
                    while (this.FInputCount[0] < FInputs.Count)
                    {
                        this.FInputs[this.FInputs.Count - 1].Dispose();
                        this.FInputs.RemoveAt(this.FInputs.Count - 1);
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
