using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils;
using VVVV.Utils.VMath;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using Microsoft.Kinect;
using System.Runtime.InteropServices;

namespace VVVV.DX11.Nodes.MSKinect
{
    [PluginInfo(Name = "RayTable", 
	            Category = "Kinect2", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "DX11, texture",
                Help = "Returns a G32R32F ray texture node")]
    public unsafe class KinectRayTextureNode : KinectBaseTextureNode
    {
        private PointF[] data;

        private int width;
        private int height;

        [Input("Update", IsBang = true)]
        protected ISpread<bool> update;

        [ImportingConstructor()]
        public KinectRayTextureNode(IPluginHost host)
        {
            this.InitBuffers();
        }

        private void InitBuffers()
        {
            this.width = 512;
            this.height = 424;
        }

        protected override void OnEvaluate()
        {
            base.OnEvaluate();

            if (this.update[0] && this.runtime != null)
            {
                this.data = this.runtime.Runtime.CoordinateMapper.GetDepthFrameToCameraSpaceTable();
                this.FInvalidate = true;
            }
        }


        protected override int Width
        {
            get { return this.width; }
        }

        protected override int Height
        {
            get { return this.height; }
        }

        protected override SlimDX.DXGI.Format Format
        {

            get { return SlimDX.DXGI.Format.R32G32_Float; }
        }

        protected unsafe override void CopyData(DX11DynamicTexture2D texture)
        {
            lock (m_lock)
            {
                fixed (PointF* ptr = &this.data[0])
                {
                    texture.WriteData(new IntPtr(ptr), this.width * this.height * 8);
                }
            }
        }

        protected override void OnRuntimeConnected()
        {

        }

        protected override void OnRuntimeDisconnected()
        {

        }

        protected override void Disposing()
        {
        }

    }
}



                      