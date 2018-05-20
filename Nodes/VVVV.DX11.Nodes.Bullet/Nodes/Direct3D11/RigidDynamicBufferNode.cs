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


namespace VVVV.DX11.Nodes.Bullet
{
    [PluginInfo(Name = "DynamicBuffer", Category = "DX11",Version="Bullet.RigidBody",
        Help = "Retrieves details for a rigid body", Author = "vux")]
    public unsafe class BulletRigidDynamicBufferNode : IPluginEvaluate, IDX11ResourceHost, System.IDisposable
    {
        [Input("Bodies")]
        protected Pin<RigidBody> FBodies;

        [Input("Buffer Count", DefaultValue=1024)]
        protected Pin<int> FBufferCount;

        [Input("Use Motion State", Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
        protected ISpread<bool> FUseMotionState;


        [Output("Body Transforms")]
        protected ISpread<DX11Resource<DX11DynamicStructuredBuffer<Matrix>>> FOutBody;

        public void Evaluate(int SpreadMax)
        {
            if (this.FBodies.IsConnected)
            {
                this.FOutBody.SliceCount = 1;
                if (this.FOutBody[0] == null) { this.FOutBody[0] = new DX11Resource<DX11DynamicStructuredBuffer<Matrix>>(); }

            }
            else
            {
                this.FOutBody.SliceCount = 0;
                
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FOutBody.SliceCount > 0)
            {
                if (this.FOutBody[0].Contains(context))
                {
                    if (this.FOutBody[0][context].ElementCount != this.FBufferCount[0])
                    {
                        this.FOutBody[0].Dispose(context);
                    }
                }

                bool useMotionState = this.FUseMotionState[0];

                if (!this.FOutBody[0].Contains(context))
                {
                    this.FOutBody[0][context] = new DX11DynamicStructuredBuffer<Matrix>(context, this.FBufferCount[0]);
                }

                    int elem = Math.Min(this.FBufferCount[0], this.FBodies.SliceCount);

                    Matrix[] mat = new Matrix[elem];
                    for (int i = 0; i < elem; i++)
                    {
                        Matrix m = useMotionState ? this.FBodies[i].MotionState.WorldTransform : this.FBodies[i].WorldTransform;
                        Vector3 v = this.FBodies[i].CollisionShape.LocalScaling;
                        m = Matrix.Scaling(v) * m;

                        mat[i] = Matrix.Transpose(m);
                    }

                    if (elem > 0)
                    {

                        this.FOutBody[0][context].WriteData(mat);
                    }
                
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutBody.SafeDisposeAll(context);
        }

        public void Dispose()
        {
            this.FOutBody.SafeDisposeAll();
        }
    }
}
