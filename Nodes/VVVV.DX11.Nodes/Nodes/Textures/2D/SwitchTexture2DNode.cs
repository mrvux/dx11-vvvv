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
    [PluginInfo(Name="Switch", Category="DX11.Texture", Version="2d", Author="vux")]
    public class SwitchTexture2DNode : IPluginEvaluate, IDX11ResourceHost, IPartImportsSatisfiedNotification
    {
        [Input("Switch", Order=-5)]
        protected ISpread<int> FInSwitch;

        [Config("Input Count", DefaultValue = 2, MinValue = 2)]
        protected IDiffSpread<int> FInputCount;

        [Output("Output")]
        protected ISpread<DX11Resource<DX11Texture2D>> FOutput;

        private List<IIOContainer<Pin<DX11Resource<DX11Texture2D>>>> FInputs = new List<IIOContainer<Pin<DX11Resource<DX11Texture2D>>>>();

        [Import()]
        protected IPluginHost FHost;

        [Import()]
        protected IIOFactory FIOFactory;

        public void Evaluate(int SpreadMax)
        {
            

            if (this.FInSwitch.SliceCount == 1)
            {
                int idx = VMath.Zmod(FInSwitch[0], FInputs.Count);
                var pin = FInputs[idx].IOObject;

                if (pin.SliceCount > 0)
                {
                    this.FOutput.SliceCount = pin.SliceCount;
                    for (int i = 0; i < pin.SliceCount; i++)
                    {
                        this.FOutput[i] = pin.IsConnected ? pin[i] : new DX11Resource<DX11Texture2D>();
                    }
                }
                else
                {
                    this.FOutput.SliceCount = SpreadMax;
                }
            }
            else
            {
                this.FOutput.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    int idx = VMath.Zmod(FInSwitch[i], FInputs.Count);

                    var pin = FInputs[idx].IOObject;

                    this.FOutput[i] = pin.IsConnected ? pin[i] : new DX11Resource<DX11Texture2D>();
                }
            }
        }

        public void Update(DX11RenderContext context)
        {
        }

        public void Destroy(DX11RenderContext context, bool force)
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
                        IIOContainer<Pin<DX11Resource<DX11Texture2D>>> newlayer = this.FIOFactory.CreateIOContainer<Pin<DX11Resource<DX11Texture2D>>>(attr);
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
