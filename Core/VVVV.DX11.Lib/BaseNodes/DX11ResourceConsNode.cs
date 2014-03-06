﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins;
using VVVV.DX11.Lib.Devices;
using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Nodes
{
    public class DX11ResourceConsNode<T> : IPluginEvaluate, IDX11ResourceProvider, IPartImportsSatisfiedNotification where T : IDX11Resource
    {
        [Config("Input Count", DefaultValue = 2, MinValue = 2)]
        protected IDiffSpread<int> FInputCount;

        [Output("Output")]
        protected ISpread<ISpread<DX11Resource<T>>> FOutput;

        private List<IIOContainer<Pin<DX11Resource<T>>>> FInputs = new List<IIOContainer<Pin<DX11Resource<T>>>>();

        protected virtual string InputPinName { get { return "Input"; } }

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
                    if (this.FInputs[i].IOObject.PluginIO.IsConnected)
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
                        InputAttribute attr = new InputAttribute(this.InputPinName + " " + Convert.ToString(this.FInputs.Count + 1));
                        //attr.IsSingle = true;
                        attr.CheckIfChanged = true;
                        //Create new layer Pin
                        IIOContainer<Pin<DX11Resource<T>>> newlayer = this.FIOFactory.CreateIOContainer<Pin<DX11Resource<T>>>(attr);
                        newlayer.IOObject.SliceCount = 1;
                        this.FInputs.Add(newlayer);
                        newlayer.IOObject[0] = new DX11Resource<T>();
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

        #region IDX11ResourceProvider Members
        public void Update(IPluginIO pin, DX11RenderContext OnDevice)
        {

        }

        public void Destroy(IPluginIO pin, DX11RenderContext OnDevice, bool force)
        {

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
