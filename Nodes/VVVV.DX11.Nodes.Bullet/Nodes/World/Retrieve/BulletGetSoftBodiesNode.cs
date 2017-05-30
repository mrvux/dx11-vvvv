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
    [PluginInfo(Name = "SoftBodies", Category = "Bullet", Version ="", Author = "vux")]
    public class BulletGetSoftBodiesBodiesNode : IPluginEvaluate
    {
        [Input("World", IsSingle = true)]
        protected ISpread<ISoftBodyCollection> FWorld;

        [Output("Soft Bodies")]
        protected ISpread<SoftBody> FSoftBodies;

        public void Evaluate(int SpreadMax)
        {
            if (this.FWorld[0] != null)
            {
                var bodies = this.FWorld[0].SoftBodies;
                this.FSoftBodies.SliceCount = bodies.Count;

                var outputBuffer = this.FSoftBodies.Stream.Buffer;
                for (int i = 0; i < bodies.Count; i++)
                {
                    outputBuffer[i] = bodies[i];
                }
                this.FSoftBodies.Flush(true);
            }
            else
            {
                this.FSoftBodies.SliceCount = 0;
            }
        }
    }
}
