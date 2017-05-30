using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using BulletSharp;
using BulletSharp.SoftBody;
using VVVV.DataTypes.Bullet;
using VVVV.Bullet.DataTypes;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "RigidBodies", Category = "Bullet", Version ="", Author = "vux")]
    public class BulletGetRigidBodiesWorldNode : IPluginEvaluate
    {
        [Input("World", IsSingle = true)]
        protected ISpread<IRigidBodyCollection> FWorld;

        [Output("Rigid Bodies")]
        protected ISpread<RigidBody> FRigidBodies;

        public void Evaluate(int SpreadMax)
        {
            if (this.FWorld[0] != null)
            {
                var bodies = this.FWorld[0].RigidBodies;
                this.FRigidBodies.SliceCount = bodies.Count;

                var outputBuffer = this.FRigidBodies.Stream.Buffer;
                for (int i = 0; i < bodies.Count; i++)
                {
                    outputBuffer[i] = bodies[i];
                }
                this.FRigidBodies.Flush(true);
            }
            else
            {
                this.FRigidBodies.SliceCount = 0;
            }
        }
    }
}
