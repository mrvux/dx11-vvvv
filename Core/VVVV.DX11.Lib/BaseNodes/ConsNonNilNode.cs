using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11.Lib;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins;
using VVVV.DX11.Lib.Devices;
using FeralTic.Resources;

namespace VVVV.DX11.Nodes
{
    public class ConsNonNilNode<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Config("Input Count", DefaultValue = 2, MinValue = 2)]
        protected IDiffSpread<int> FInputCount;

        [Output("Output")]
        protected ISpread<ISpread<T>> FOutput;

        private List<IIOContainer<Pin<T>>> FInputs = new List<IIOContainer<Pin<T>>>();

        [Import()]
        protected IPluginHost FHost;

        [Import()]
        protected IIOFactory FIOFactory;

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = this.FInputs.Count;

            for (int i = 0; i < FInputs.Count; i++)
            {
                if (this.FInputs[i].IOObject.IsChanged)
                {
                    if (this.FInputs[i].IOObject.IsConnected)
                    {
                        this.FOutput[i].SliceCount = this.FInputs[i].IOObject.SliceCount;
                        this.FOutput[i] = this.FInputs[i].IOObject;
                    }
                    else
                    {
                        this.FOutput[i].SliceCount = 0;
                    }
                }
            }
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
                        IIOContainer<Pin<T>> newlayer = this.FIOFactory.CreateIOContainer<Pin<T>>(attr);
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
