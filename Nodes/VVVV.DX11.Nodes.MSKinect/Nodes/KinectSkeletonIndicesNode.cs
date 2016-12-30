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
    [PluginInfo(Name = "Skeleton", 
	            Category = "DX11.Geometry", 
	            Version = "Lines", 
	            Author = "vux", 
	            Tags = "DX11, kinect",
	            Help = "Returns a line-based skeleton geometry")]
    public class KinectSkeletonIndicesMeshNode : IPluginEvaluate, IDX11ResourceProvider, IDisposable
    {
        [Output("Output", IsSingle = true)]
        Pin<DX11Resource<DX11IndexOnlyGeometry>> FOutput;

        [Output("Arms Geom", IsSingle = true)]
        Pin<DX11Resource<DX11IndexOnlyGeometry>> FOutArmsGeom;

        [Output("Legs Geom", IsSingle = true)]
        Pin<DX11Resource<DX11IndexOnlyGeometry>> FOutLegsGeom;

        [Output("Arms")]
        ISpread<int> FOutArms;

        [Output("Up Body")]
        ISpread<int> FOutUpBody;

        [Output("Legs")]
        ISpread<int> FOutLegs;

        [Output("Chest")]
        ISpread<int> FOutChest;

        bool first = true;

        public void Evaluate(int SpreadMax)
        {

            if (first)
            {
                this.FOutArms.AssignFrom(KinectRuntime.SKELETON_ARMS);
                this.FOutChest.AssignFrom(KinectRuntime.SKELETON_CHEST);
                this.FOutLegs.AssignFrom(KinectRuntime.SKELETON_LEGS);
                this.FOutUpBody.AssignFrom(KinectRuntime.SKELETON_UPBODY);

                this.FOutput[0] = new DX11Resource<DX11IndexOnlyGeometry>();
                this.FOutArmsGeom[0] = new DX11Resource<DX11IndexOnlyGeometry>();
                this.FOutLegsGeom[0] = new DX11Resource<DX11IndexOnlyGeometry>();

                first = false;
            }
        }

        private void BuildBuffer(DX11RenderContext context,int[] data, Pin<DX11Resource<DX11IndexOnlyGeometry>> respin)
        {
            DX11IndexOnlyGeometry geom = new DX11IndexOnlyGeometry(context);

            geom.HasBoundingBox = false;
            geom.Topology = PrimitiveTopology.LineList;
            geom.InputLayout = new InputElement[0];

            var indexstream = new DataStream(data.Length * 4, true, true);
            indexstream.WriteRange(data);
            indexstream.Position = 0;

            geom.IndexBuffer = new DX11IndexBuffer(context, indexstream, false, true);
            respin[0][context] = geom;
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (!this.FOutput[0].Contains(context))
            {
                this.BuildBuffer(context, KinectRuntime.SKELETON_INDICES, this.FOutput);
                this.BuildBuffer(context, KinectRuntime.SKELETON_ARMS, this.FOutArmsGeom);
                this.BuildBuffer(context, KinectRuntime.SKELETON_LEGS, this.FOutLegsGeom);

            }
            
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                this.FOutput[i].Dispose(context);
            }
        }

        public void Dispose()
        {
            this.DisposeOutput();
        }

        private void DisposeOutput()
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                this.FOutput[i].Dispose();
            }
        }
    }
}
