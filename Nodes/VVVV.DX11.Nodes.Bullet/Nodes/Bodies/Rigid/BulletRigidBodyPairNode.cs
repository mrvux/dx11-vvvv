using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;
using VVVV.Internals.Bullet;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="BuildPair",Category="Bullet", Version ="RigidBody",
		Help = "Builds a rigid body pair", Author = "vux")]
	public unsafe class BulletBuildBodyPairNode : IPluginEvaluate
	{
		[Input("Body 1")]
        protected ISpread<RigidBody> bodies1;

        [Input("Body 2")]
        protected ISpread<RigidBody> bodies2;

        [Input("Collide Connected")]
        protected ISpread<bool> collideConnected;

        [Output("Output")]
        protected ISpread<RigidBodyPair> output;

		public void Evaluate(int SpreadMax)
		{
            this.output.SliceCount = SpreadMax;

            var buffer = this.output.Stream.Buffer;
            for (int i = 0; i < SpreadMax; i++)
            {
                this.output[i] = new RigidBodyPair();
                this.output[i].body1 = this.bodies1[i];
                this.output[i].body2 = this.bodies2[i];
                this.output[i].collideConnected = this.collideConnected[i];
            }
            this.output.Flush(true);
		}
	}
}
