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
using VVVV.Bullet.DataTypes;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "DynamicsProperties", Category = "Bullet", Version ="Join",
        Help = "Builds bullet rigid body properties", Author = "vux")]
    public unsafe class BulletRigidBodyPropertiesJoinNode : IPluginEvaluate
    {
        [Input("Friction", DefaultValue =0.1)]
        protected ISpread<float> friction;

        [Input("Restitution", DefaultValue =0.5)]
        protected ISpread<float> restitution;

        [Input("Rolling Friction", DefaultValue = 0.1)]
        protected ISpread<float> rollingFriction;

        [Input("Is Active", DefaultValue =1)]
        protected ISpread<bool> isActive;

        [Input("Has Contact Response", DefaultValue = 1)]
        protected ISpread<bool> hasContactResponse;

        [Input("Debug View Enabled", DefaultValue = 1)]
        protected ISpread<bool> debugViewEnabled;

        [Output("Output")]
        protected ISpread<RigidBodyProperties> output;

        public void Evaluate(int SpreadMax)
        {
            this.output.SliceCount = SpreadMax;
            
            fixed (RigidBodyProperties* posePtr = &this.output.Stream.Buffer[0])
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    posePtr[i].Friction = friction[i];
                    posePtr[i].Restitution = restitution[i];
                    posePtr[i].RollingFriction = rollingFriction[i];
                    posePtr[i].IsActive = isActive[i];
                    posePtr[i].HasContactResponse = hasContactResponse[i];
                    posePtr[i].DebugViewEnabled = debugViewEnabled[i];
                }
            }
            this.output.Flush(true);
        }
    }
}
