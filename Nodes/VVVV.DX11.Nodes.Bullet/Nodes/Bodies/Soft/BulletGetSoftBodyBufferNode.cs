using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11;

using BulletSharp;
using BulletSharp.SoftBody;

namespace VVVV.DX11.Nodes.Bullet
{
    [PluginInfo(Name = "GetSoftBodyBuffer", Category = "Bullet",
        Help = "Gets some info about a soft body", Author = "vux")]
    public class BulletGetSoftBodyBufferNode : IPluginEvaluate, IDX11ResourceHost, System.IDisposable
    {
        [Input("Bodies")]
        protected ISpread<SoftBody> FBodies;

        [Output("Nodes Count")]
        protected ISpread<int> FOutCount;

        [Output("Position Buffer", IsSingle=true)]
        protected ISpread<DX11Resource<DX11DynamicStructuredBuffer<Vector3>>> FOutNodes;

        private List<Vector3> nodepos = new List<Vector3>();

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutNodes[0] == null) { this.FOutNodes[0] = new DX11Resource<DX11DynamicStructuredBuffer<Vector3>>();}

            nodepos.Clear();

            this.FOutCount.SliceCount = this.FBodies.SliceCount;

            for (int i = 0; i < SpreadMax; i++)
            {

                SoftBody sb = this.FBodies[i];
                this.FOutCount[i] = sb.Nodes.Count;
               
                //sb.Nodes[0].

                AlignedNodeArray nodes = sb.Nodes;
                for (int j = 0; j < sb.Nodes.Count; j++)
                {
                    nodepos.Add(nodes[j].X);
                    //this.FOutNodes[i][j] = sb.Nodes[j].X.ToVVVVector();
                }

            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FOutNodes[0].Contains(context))
            {
                if (this.nodepos.Count != this.FOutNodes[0][context].ElementCount)
                {
                    this.FOutNodes[0].Dispose(context);
                }
            }

            if (!this.FOutNodes[0].Contains(context))
            {
                if (this.nodepos.Count > 0)
                {
                    this.FOutNodes[0][context] = new DX11DynamicStructuredBuffer<Vector3>(context, this.nodepos.Count);
                }
            }

            if (this.nodepos.Count > 0)
            {
                this.FOutNodes[0][context].WriteData(this.nodepos.ToArray());
            }

           
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (this.FOutNodes.SliceCount > 0)
            {
                if (this.FOutNodes[0] != null)
                {
                    this.FOutNodes[0].Dispose(context);
                    this.FOutNodes[0] = null;
                }
            }
        }

        public void Dispose()
        {
            if (this.FOutNodes.SliceCount > 0)
            {
                if (this.FOutNodes[0] != null)
                {
                    this.FOutNodes[0].Dispose();
                    this.FOutNodes[0] = null;
                }
            }
        }
    }
}

