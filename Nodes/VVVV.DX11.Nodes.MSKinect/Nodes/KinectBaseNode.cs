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

using VVVV.MSKinect.Lib;

namespace VVVV.DX11.Nodes.MSKinect
{
    public abstract class KinectBaseNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        private Pin<KinectRuntime> FInRuntime;


        private bool FInvalidateConnect = false;
        protected bool FInvalidate = true;

        protected bool Resized = false;

        protected KinectRuntime runtime;

        protected object m_lock = new object();

        protected abstract void OnRuntimeConnected();
        protected abstract void OnRuntimeDisconnected();

        protected virtual void OnEvaluate() { }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.OnRuntimeDisconnected();
                }

                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];
                    this.OnRuntimeConnected();
                }
                else
                {
                    this.OnRuntimeDisconnected();
                }

                this.FInvalidateConnect = false;
            }

            this.OnEvaluate();


        }

        public void ConnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        public void DisconnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }
    }
}
