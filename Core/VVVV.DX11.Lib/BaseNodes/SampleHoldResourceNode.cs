using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    public abstract class SampleHoldResourceNode<T> : IPluginEvaluate, IDX11ResourceHost, IDX11UpdateBlocker, IDisposable where T : IDX11Resource
    {
       [Input("Input", AutoValidate =false)]
        protected Pin<DX11Resource<T>> input;

        [Input("Set", IsBang =true, DefaultValue =1, IsSingle =true)]
        protected ISpread<bool> apply;

        [Input("Set On Create", DefaultValue = 1, IsSingle = true, Visibility =PinVisibility.OnlyInspector)]
        protected ISpread<bool> setoncreate;

        [Input("Flush", DefaultValue = 0, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<bool> flush;

        [Output("Output")]
        protected ISpread<DX11Resource<T>> output;

        private bool first = true;

        private bool needCopy;

        private T currentResource;

        public bool Enabled
        {
            get { return this.needCopy; }
        }

        public void Evaluate(int SpreadMax)
        {
            this.needCopy = false;

            if (this.apply[0] || (this.first && this.setoncreate[0]))
            {
                this.input.Sync();

                //Destroy resource on nil
                if (SpreadMax == 0 && this.output.SliceCount == 1)
                {
                    this.output.SafeDisposeAll();
                }

                this.output.SliceCount = SpreadMax == 0 ? 0 : 1;
                this.output.CreateIfNull();

                this.needCopy = this.output.SliceCount > 0; // need copy only if input is not nil

            }

            this.first = false;
        }

        public void Update(DX11RenderContext context)
        {
            //Note no need to check if copy needed, as blocker will auto handle that
            if (this.input[0].Contains(context))
            {
                var inputResource = this.input[0][context];

                if (inputResource != null)
                {
                    this.ProcessResource(context, this.input[0][context], ref this.currentResource);
                    if (this.currentResource != null)
                    {
                        this.output[0][context] = this.currentResource;
                    }
                    else
                    {
                        this.output[0].Remove(context);
                    }
                }
                else
                {
                    this.output.SafeDisposeAll(context);
                }

                if (this.flush[0])
                {
                    context.CurrentDeviceContext.Flush();
                }
            }
        }

        protected abstract void ProcessResource(DX11RenderContext context, T inputResource, ref T outputResource);

        public void Destroy(DX11RenderContext context, bool force)
        {

        }

        public void Dispose()
        {
            this.output.SafeDisposeAll();
        }

    }
}
